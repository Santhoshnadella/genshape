using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Numerics;
using PicoGK;
using Leap71.ShapeKernel;
using Leap71.FusorExample;
using Leap71.HabitatExample;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutomotiveEngineering
{
    public class GenShapeServer
    {
        private HttpListener _listener;
        private bool _isRunning;
        private Library? _library;

        public GenShapeServer()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:5000/");
            _listener.Prefixes.Add("http://127.0.0.1:5000/");
        }

        public void Start()
        {
            // Initialize single, long-lived computational workspace context!
            _library = new Library(0.5f);

            _isRunning = true;
            _listener.Start();
            Console.WriteLine("==================================================");
            Console.WriteLine("   GenShape HTTP API Server Started on Port 5000");
            Console.WriteLine("   Listening at: http://localhost:5000/");
            Console.WriteLine("==================================================");

            ThreadPool.QueueUserWorkItem((state) =>
            {
                while (_isRunning)
                {
                    try
                    {
                        HttpListenerContext context = _listener.GetContext();
                        ThreadPool.QueueUserWorkItem((c) => HandleRequest((HttpListenerContext)c), context);
                    }
                    catch (Exception ex)
                    {
                        if (!_isRunning) break;
                        Console.WriteLine($"Server listening exception: {ex.Message}");
                    }
                }
            });
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();

            // Clear voxel allocations and finalize engine cleanly
            _library?.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine("GenShape HTTP API Server Stopped.");
        }

        private void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // Enforce CORS standard headers
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
            response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");

            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close();
                return;
            }

            if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/api/generate")
            {
                try
                {
                    string requestBody;
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        requestBody = reader.ReadToEnd();
                    }

                    GenerateRequest? req = null;
                    try 
                    {
                        req = JsonSerializer.Deserialize<GenerateRequest>(requestBody);
                    }
                    catch (JsonException jsonEx)
                    {
                        Console.WriteLine($"[API Warning] Malformed JSON payload received: {jsonEx.Message}");
                        SendError(response, "Malformed JSON request payload.", HttpStatusCode.BadRequest);
                        return;
                    }

                    if (req == null || string.IsNullOrWhiteSpace(req.prompt))
                    {
                        SendError(response, "Invalid JSON request payload or empty prompt.", HttpStatusCode.BadRequest);
                        return;
                    }

                    // Enforce Hard Bounds to prevent Native C++ Memory Overflow
                    req.volume = Math.Clamp(req.volume, 10, 50000); // Max 50 Liters
                    req.infillDensity = Math.Clamp(req.infillDensity, 0.05f, 0.95f);
                    req.loadForce = Math.Clamp(req.loadForce, 1f, 100000f);

                    Console.WriteLine($"[API] Generation Request Received: {req.prompt}");
                    Console.WriteLine($"      Material: {req.material}, SafetyCritical: {req.safetyCritical}, Infill: {req.infillType} ({req.infillDensity:P0})");

                    // Trigger Voxel compilation inside a clean background thread running PicoGK
                    var result = CompileVoxelPart(req);

                    string jsonResponse = JsonSerializer.Serialize(result);
                    byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[API ERROR] {ex}");
                    SendError(response, $"Internal server error during geometry generation: {ex.Message}", HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                SendError(response, "Endpoint not found or method not supported.", HttpStatusCode.NotFound);
            }
        }

        private void SendError(HttpListenerResponse response, string message, HttpStatusCode status)
        {
            try
            {
                var errObj = new { error = message };
                string json = JsonSerializer.Serialize(errObj);
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                response.StatusCode = (int)status;
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();
            }
            catch { }
        }

        // Runs the PicoGK & ShapeKernel computational pipeline in headless mode
        private GenerateResponse CompileVoxelPart(GenerateRequest req)
        {
            string partType = "bracket";
            string promptLower = req.prompt.ToLower();

            if (promptLower.Contains("chassis") || promptLower.Contains("car") || promptLower.Contains("tub") || promptLower.Contains("frame"))
            {
                partType = "chassis";
            }
            else if (promptLower.Contains("habitat") || promptLower.Contains("dome") || promptLower.Contains("origami") || promptLower.Contains("tent"))
            {
                partType = "habitat";
            }
            else if (promptLower.Contains("fusor") || promptLower.Contains("reactor") || promptLower.Contains("chamber") || promptLower.Contains("grid"))
            {
                partType = "fusor";
            }
            else if (promptLower.Contains("wheel") || promptLower.Contains("rover") || promptLower.Contains("tire") || promptLower.Contains("crawler"))
            {
                partType = "wheel";
            }
            else if (promptLower.Contains("bolt") || promptLower.Contains("screw") || promptLower.Contains("fastener") || promptLower.Contains("thread"))
            {
                partType = "bolt";
            }

            float volumeCubicMM = 0f;
            float boxWidth = 0f;
            float boxHeight = 0f;
            float boxDepth = 0f;

            // Target export file path inside the React public folder
            string exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../GenShapeUI/public/generated_part.stl");
            
            // Clean up relative path if compiled
            string absoluteExportPath = Path.GetFullPath(exportPath);
            string exportDir = Path.GetDirectoryName(absoluteExportPath);
            if (!Directory.Exists(exportDir))
            {
                Directory.CreateDirectory(exportDir);
            }

            // Define wall thickness calculations based on Material & Safety-critical metrics
            float baseThickness = 2.0f; // in mm
            if (req.material == 0) baseThickness = 1.8f;      // Steel
            else if (req.material == 1) baseThickness = 2.5f; // Aluminum
            else if (req.material == 2) baseThickness = 4.0f; // Plastics
            else if (req.material == 3) baseThickness = 3.5f; // Composite
            else if (req.material == 4) baseThickness = 2.0f; // Titanium

            if (req.safetyCritical)
            {
                baseThickness *= 1.5f; // Add safety margin factors
            }

            // Headless initialization of the PicoGK Geometry Engine!
            // Determine voxel resolution based on precision demands
            // Enforce hard-floor bounding so we don't accidentally allocate 100GB of RAM
            float voxelSize = 0.8f;
            if (req.prompt.Contains("bolt") || req.prompt.Contains("screw") || req.prompt.Contains("thread"))
            {
                voxelSize = 0.3f; // 300 microns for extreme thread and helix accuracy
            }
            // Note: In a true production deployment, Library.Go() should pass this voxelSize dynamically.

            Voxels voxPart = new Voxels();

                // Dynamic C# Code Generation based on engineering intent and requirements
                string dynamicCSharp = BuildDynamicCSharpCode(req.prompt, baseThickness);
                Console.WriteLine("==================================================");
                Console.WriteLine("[Agent 2] Dynamically Generated C# CAD Script:");
                Console.WriteLine(dynamicCSharp);
                Console.WriteLine("==================================================");

                // Dynamically compile and execute the C# code in-memory using Roslyn
                voxPart = CompileAndExecuteDynamicPart(dynamicCSharp);

                // FEATURE 1: Voxel-Based Lattice Infill Engine Integration
                if (req.infillType > 0 && req.infillDensity > 0.05f)
                {
                    // Hollow out a core zone to receive infill
                    float shellThickness = baseThickness * 1.5f;
                    Voxels voxOuter = new Voxels(voxPart);
                    
                    // Create an offset inner void for the infill
                    Voxels voxInnerVoid = new Voxels(voxPart);
                    voxInnerVoid.Offset(-shellThickness);

                    voxInnerVoid.CalculateProperties(out float fInnerVol, out BBox3 innerBBox);
                    if (!innerBBox.bIsEmpty())
                    {
                        IImplicit sdfPattern;
                        float unitSize = 15f; // mm length after which the gyroid repeats
                        float thicknessRatio = req.infillDensity * 2.5f; // Map density multiplier

                        if (req.infillType == 1) // Gyroid
                        {
                            sdfPattern = new ImplicitGyroid(unitSize, thicknessRatio);
                        }
                        else // Diamond or custom lattice infill
                        {
                            sdfPattern = new ImplicitDiamond(unitSize, thicknessRatio);
                        }

                        // Intersect the inner void with the periodic lattice pattern
                        Voxels voxLatticeCore = new Voxels(voxInnerVoid);
                        voxLatticeCore.voxIntersectImplicit(sdfPattern);

                        // Combine the outer solid shell and the inner structural lattice infill!
                        voxPart = voxOuter;
                        voxPart.BoolSubtract(voxInnerVoid); // Empty out interior
                        voxPart.BoolAdd(voxLatticeCore);    // Inject lattice struts
                    }
                }

                // Export mesh directly to public STL
                voxPart.CalculateProperties(out volumeCubicMM, out BBox3 bbox);
                boxWidth = bbox.vecMax.X - bbox.vecMin.X;
                boxHeight = bbox.vecMax.Y - bbox.vecMin.Y;
                boxDepth = bbox.vecMax.Z - bbox.vecMin.Z;

                // Save to STL file using ShapeKernel with a bulletproof swap & retry loop
                int maxRetries = 5;
                bool saveSuccess = false;
                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        string tempPath = absoluteExportPath + ".tmp";
                        if (File.Exists(tempPath)) File.Delete(tempPath);
                        
                        Sh.ExportVoxelsToSTLFile(voxPart, tempPath);
                        
                        if (File.Exists(absoluteExportPath))
                        {
                            File.Delete(absoluteExportPath);
                        }
                        File.Move(tempPath, absoluteExportPath);
                        Console.WriteLine($"[API] Procedural STL Saved successfully to: {absoluteExportPath}");
                        saveSuccess = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[API File System Warning] Retry {i + 1}/{maxRetries} failed: {ex.Message}");
                        if (i == maxRetries - 1)
                        {
                            // Fallback: Try writing directly if swap failed
                            try
                            {
                                Sh.ExportVoxelsToSTLFile(voxPart, absoluteExportPath);
                                Console.WriteLine($"[API Fallback Direct Save] Saved STL to: {absoluteExportPath}");
                                saveSuccess = true;
                            }
                            catch (Exception directEx)
                            {
                                Console.WriteLine($"[API FATAL FILE LOCK ERROR] Could not save STL: {directEx.Message}");
                            }
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(200); // Backoff wait for handles to release
                        }
                    }
                }
                Console.WriteLine($"      Volume: {volumeCubicMM:F2} mm3, Bounding Box: {boxWidth:F1}x{boxHeight:F1}x{boxDepth:F1} mm");

                // Explicit sweep of temporary sub-voxels to prevent native C++ heap leaks
                GC.Collect();
                GC.WaitForPendingFinalizers();

            // FEATURE 3 & 4: Multi-Dimensional Physics, Cost, and Sustainability LCA scoring
            float volumeCm3 = volumeCubicMM / 1000f;
            float materialDensity = 2.7f; // default Aluminum
            float carbonCoef = 8.5f; // kg CO2 / kg material
            float baseCostPerKg = 5.0f;

            string materialName = "Aluminum";
            if (req.material == 0) { materialDensity = 7.85f; carbonCoef = 1.9f; baseCostPerKg = 2.5f; materialName = "Steel"; }
            else if (req.material == 1) { materialDensity = 2.7f; carbonCoef = 8.5f; baseCostPerKg = 6.0f; materialName = "Aluminum"; }
            else if (req.material == 2) { materialDensity = 1.05f; carbonCoef = 3.2f; baseCostPerKg = 4.0f; materialName = "Plastics (ABS)"; }
            else if (req.material == 3) { materialDensity = 1.8f; carbonCoef = 14.5f; baseCostPerKg = 25.0f; materialName = "Composite (Carbon)"; }
            else if (req.material == 4) { materialDensity = 4.5f; carbonCoef = 12.0f; baseCostPerKg = 35.0f; materialName = "Titanium"; }

            float weightKg = (volumeCm3 * materialDensity) / 1000f;
            if (weightKg < 0.001f) weightKg = 0.15f; // safety bounds

            // Cost Calculations
            float setupFee = 250f; // base amortization setup
            float processingFee = volumeCm3 * 0.35f;
            if (req.material == 3) processingFee = volumeCm3 * 1.5f; // composite layup fees
            else if (req.material == 4) processingFee = volumeCm3 * 2.2f; // titanium machining cycle fees

            float materialCost = weightKg * baseCostPerKg;
            float rawCostPerUnit = materialCost + processingFee;
            float totalEstimatedCost = rawCostPerUnit + (setupFee / req.volume);

            // LCA Sustainability
            float carbonFootprint = weightKg * carbonCoef;
            float recyclabilityIndex = 0.90f;
            if (req.material == 0) recyclabilityIndex = 0.95f;
            else if (req.material == 2) recyclabilityIndex = 0.15f;
            else if (req.material == 3) recyclabilityIndex = 0.05f;
            else if (req.material == 4) recyclabilityIndex = 0.80f;

            // Stress Analysis Simulations
            // SF = YieldStrength / EstimatedStress
            float yieldStrengthMPa = 250f; // Aluminum
            if (req.material == 0) yieldStrengthMPa = 350f;
            else if (req.material == 2) yieldStrengthMPa = 40f;
            else if (req.material == 3) yieldStrengthMPa = 600f;
            else if (req.material == 4) yieldStrengthMPa = 900f;

            float crossSectionArea = (boxWidth * boxDepth) * 0.4f; // estimate hollow ratio
            if (crossSectionArea < 1f) crossSectionArea = 100f;

            float appliedStress = req.loadForce / (crossSectionArea / 100f); // stress in MPa
            if (req.loadCase == 2) appliedStress *= 1.8f; // Torsional load multipliers

            float safetyFactor = yieldStrengthMPa / appliedStress;
            if (safetyFactor > 15f) safetyFactor = 15f;
            if (safetyFactor < 0.1f) safetyFactor = 0.1f;

            // DFM / DFA Scoring Calculations
            float dfmScore = 90f;
            float dfaScore = 88f;

            // DFM Dips
            if (req.material == 3 && req.volume > 20000) dfmScore -= 20f; // composite high volume cycle penalty
            if (req.infillType > 0) dfmScore -= 5f; // high complexity print penalty
            if (req.material == 4) dfmScore -= 10f; // titanium machining wear penalty

            // DFA Dips
            if (req.safetyCritical) dfaScore -= 8f; // high documentation/fastener double locking
            if (req.infillType > 0) dfaScore += 4f; // part consolidation improves DFA!

            dfmScore = Math.Clamp(dfmScore, 40f, 100f);
            dfaScore = Math.Clamp(dfaScore, 40f, 100f);

            var resp = new GenerateResponse();
            resp.partType = partType;
            resp.volumeCm3 = volumeCm3;
            resp.weightKg = weightKg;
            resp.totalCost = totalEstimatedCost;
            resp.materialCost = materialCost;
            resp.processingCost = processingFee;
            resp.carbonKg = carbonFootprint;
            resp.recyclability = recyclabilityIndex;
            resp.dfmScore = dfmScore;
            resp.dfaScore = dfaScore;
            resp.safetyFactor = safetyFactor;
            resp.boxSize = $"{boxWidth:F1} x {boxHeight:F1} x {boxDepth:F1} mm";

            // Warning Checks
            if (req.material == 3 && req.volume > 15000)
                resp.warnings.Add("High volume cycle bottleneck: Composites cure cycles limit production efficiency.");
            if (req.material == 3 && req.material == 1)
                resp.warnings.Add("Galvanic corrosion warning: Directly mating carbon fibers with aluminum fasteners induces severe corrosion.");
            if (safetyFactor < 1.5f && req.safetyCritical)
                resp.warnings.Add("Critical Safety Failure: Allowable stress factor is below functional ISO thresholds.");
            if (req.infillType > 0 && req.infillDensity < 0.1f)
                resp.warnings.Add("Low density infill: Lattice density is too thin to withstand base buckling forces.");

            // Standard references from Knowledge Bank
            resp.principles.Add(new PrincipleDto { id = 1, domain = "Structural", title = "Avoid Sharp Corners", desc = "Use fillets to reduce stress concentrations.", impact = "Fatigue life increases by up to 50%." });
            resp.principles.Add(new PrincipleDto { id = 4, domain = "Structural", title = "Ribbing for Stiffness", desc = "Use infills and ribs rather than solid walls.", impact = "Saves material mass by 40%." });
            resp.principles.Add(new PrincipleDto { id = 63, domain = "Cost", title = "Cycle Time Reduction", desc = "Design structures to cool down or mill quickly.", impact = "Boosts machine throughput." });

            if (req.infillType > 0)
                resp.principles.Add(new PrincipleDto { id = 76, domain = "Weight", title = "Topology Optimization", desc = "Remove unloaded material using lattice infills.", impact = "Reduces total part weight by 30-50%." });

            return resp;
        }

        public static string BuildDynamicCSharpCode(string prompt, float baseThickness)
        {
            string cleanPrompt = prompt.ToLowerInvariant();
            
            // Simple keyword-based intent parsing
            bool isBolt = cleanPrompt.Contains("bolt") || cleanPrompt.Contains("screw") || cleanPrompt.Contains("thread") || cleanPrompt.Contains("fastener");
            bool isHabitat = cleanPrompt.Contains("habitat") || cleanPrompt.Contains("dome") || cleanPrompt.Contains("origami");
            bool isFusor = cleanPrompt.Contains("fusor") || cleanPrompt.Contains("reactor") || cleanPrompt.Contains("chamber");
            bool isWheel = cleanPrompt.Contains("wheel") || cleanPrompt.Contains("rover") || cleanPrompt.Contains("traction") || cleanPrompt.Contains("tire");

            string code = @"
using System;
using System.Numerics;
using System.Collections.Generic;
using PicoGK;
using Leap71.ShapeKernel;
using Leap71.Rover;

namespace AutomotiveEngineering
{
    public class DynamicPart
    {
        public Voxels voxConstruct()
        {
";

            if (isBolt)
            {
                code += $@"
            float shankRadius = 8f;
            float shankLength = 65f;
            float headRadius = 15f;
            float headHeight = 12f;

            // Construct cylindrical shank (shaft)
            Voxels voxBolt = new Voxels(PicoGK.Utils.mshCreateCylinder(
                new Vector3(shankRadius * 2f, shankRadius * 2f, shankLength), 
                new Vector3(0, 0, shankLength / 2f)
            ));

            // Construct hex head
            using (Voxels voxHead = new Voxels(PicoGK.Utils.mshCreateCylinder(
                new Vector3(headRadius * 2f, headRadius * 2f, headHeight), 
                new Vector3(0, 0, shankLength + headHeight / 2f)
            )))
            {{
                voxBolt.BoolAdd(voxHead);
            }}

            // Dynamic threaded helical grooves cutouts
            float threadStart = 5f;
            float threadEnd = 35f;
            float pitch = 2.5f;
            float threadDepth = 1.2f;

            for (float z = threadStart; z < threadEnd; z += pitch)
            {{
                using (Voxels threadRing = new Voxels(PicoGK.Utils.mshCreateCylinder(
                    new Vector3(shankRadius * 2.1f, shankRadius * 2.1f, 1.0f), 
                    new Vector3(0, 0, z)
                )))
                using (Voxels innerRing = new Voxels(PicoGK.Utils.mshCreateCylinder(
                    new Vector3((shankRadius - threadDepth) * 2f, (shankRadius - threadDepth) * 2f, 2.0f), 
                    new Vector3(0, 0, z)
                )))
                {{
                    threadRing.BoolSubtract(innerRing);
                    voxBolt.BoolSubtract(threadRing);
                }}
            }}

            return voxBolt;
";
            }
            else if (isHabitat)
            {
                code += $@"
            float stowedRadius = 30f;
            float deployedRadius = 50f;
            float deployedHeight = 60f;
            float wallThickness = {baseThickness}f;

            Leap71.HabitatExample.InflatableHabitat hab = new Leap71.HabitatExample.InflatableHabitat(
                1.0f, stowedRadius, deployedRadius, 5f, deployedHeight, wallThickness, 6, 16
            );
            return hab.voxConstruct();
";
            }
            else if (isFusor)
            {
                code += $@"
            float outerRad = 60f;
            float wallThick = MathF.Max(3f, {baseThickness}f);
            float outerGridRad = 40f;
            float innerGridRad = 15f;

            Leap71.FusorExample.Fusor fus = new Leap71.FusorExample.Fusor(
                outerRad, wallThick, outerGridRad, innerGridRad, 12f, 30f
            );
            return fus.voxConstruct();
";
            }
            else if (isWheel)
            {
                code += $@"
            // Dynamic Leap71 Rover Wheel procedural instantiation
            Leap71.Rover.RoverWheel wheel = new Leap71.Rover.Wheel_02(); // Default: Smooth floral lattice spoke wheel preset
            
            if (""{cleanPrompt}"".Contains(""tread"") || ""{cleanPrompt}"".Contains(""traction"") || ""{cleanPrompt}"".Contains(""front""))
            {{
                wheel = new Leap71.Rover.Wheel_01(); // High-traction tread pattern wheel preset
            }}
            else if (""{cleanPrompt}"".Contains(""hybrid"") || ""{cleanPrompt}"".Contains(""lightweight""))
            {{
                wheel = new Leap71.Rover.Wheel_03(); // Lightweight mesh variant preset
            }}
            else if (""{cleanPrompt}"".Contains(""heavy"") || ""{cleanPrompt}"".Contains(""solid""))
            {{
                wheel = new Leap71.Rover.Wheel_04(); // Heavy duty variant
            }}
            
            return wheel.voxConstruct();
";
            }
            else if (prompt.Contains("gyroid") || prompt.Contains("organic") || prompt.Contains("lattice"))
            {
                code += $@"
            // Advanced Leap71 gyroid computational pattern
            float fRadius = 35f;
            IImplicit sdfSphere = new Leap71.ShapeKernel.ImplicitSphere(Vector3.Zero, fRadius);
            IImplicit sdfPattern = new Leap71.ShapeKernel.ImplicitGyroid(3f, 1f);
            
            BBox3 oBBox = new BBox3(1.2f * new Vector3(-fRadius, -fRadius, -fRadius), 1.2f * new Vector3(fRadius, fRadius, fRadius));
            Voxels voxSphere = new Voxels(sdfSphere, oBBox);
            Voxels voxGyroid = voxSphere.voxIntersectImplicit(sdfPattern);
            return voxGyroid;
";
            }
            else if (prompt.Contains("pipe") || prompt.Contains("tube") || prompt.Contains("manifold"))
            {
                code += $@"
            // Advanced Leap71 spline modulated pipe manifold
            LocalFrame oLocalFrame = new LocalFrame(Vector3.Zero);
            BasePipe oPipe = new BasePipe(oLocalFrame, 75, 4, 30);
            oPipe.SetLengthSteps(400);
            // Sinusoidal modulation of diameter along length ratio
            oPipe.SetRadius(
                new SurfaceModulation(12f),
                new SurfaceModulation(new LineModulation((ratio) => 12f - 3f * MathF.Cos(6f * ratio)))
            );
            return oPipe.voxConstruct();
";
            }
            else // Dynamic support bracket cylinder with internal cutout
            {
                code += $@"
            float width = 60f;
            float depth = 60f;
            float height = 70f;
            float wallThick = {baseThickness}f;

            Voxels voxPart = new Voxels(PicoGK.Utils.mshCreateCylinder(new Vector3(width, depth, height), Vector3.Zero));
            using (Voxels innerCyl = new Voxels(PicoGK.Utils.mshCreateCylinder(new Vector3(width - wallThick * 2f, depth - wallThick * 2f, height + 10f), Vector3.Zero)))
            {{
                voxPart.BoolSubtract(innerCyl);
            }}
            return voxPart;
";
            }

            code += @"
        }
    }
}
";
            return code;
        }

        public static Voxels CompileAndExecuteDynamicPart(string code)
        {
            Console.WriteLine("[Roslyn] Compiling dynamic geometry C# syntax tree in-memory...");
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

            string assemblyName = Path.GetRandomFileName();
            
            // Resolve standard core .NET assemblies dynamically
            string sysRuntimeLocation = Assembly.Load("System.Runtime").Location;
            string sysNumericsLocation = Assembly.Load("System.Numerics.Vectors").Location;
            string sysConsoleLocation = Assembly.Load("System.Console").Location;

            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(sysRuntimeLocation),
                MetadataReference.CreateFromFile(sysNumericsLocation),
                MetadataReference.CreateFromFile(sysConsoleLocation),
                MetadataReference.CreateFromFile(typeof(PicoGK.Library).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Leap71.ShapeKernel.Sh).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Leap71.FusorExample.Fusor).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Leap71.HabitatExample.InflatableHabitat).Assembly.Location)
            };

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);
                if (!result.Success)
                {
                    Console.WriteLine("[Roslyn ERROR] Dynamic C# compilation failed:");
                    foreach (Diagnostic diagnostic in result.Diagnostics)
                    {
                        if (diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            Console.WriteLine($"               {diagnostic.Id}: {diagnostic.GetMessage()}");
                        }
                    }
                    throw new Exception("Roslyn compilation error.");
                }

                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());
                Type? type = assembly.GetType("AutomotiveEngineering.DynamicPart");
                if (type == null) throw new Exception("Type DynamicPart not found in generated assembly.");

                object? instance = Activator.CreateInstance(type);
                MethodInfo? method = type.GetMethod("voxConstruct");
                if (method == null) throw new Exception("voxConstruct method not found.");

                Console.WriteLine("[Roslyn SUCCESS] Dynamic assembly executed successfully!");
                var voxResult = method.Invoke(instance, null);
                if (voxResult == null) throw new Exception("voxConstruct returned null.");
                return (Voxels)voxResult;
            }
        }
    }

    public class GenerateRequest
    {
        public string prompt { get; set; } = "";
        public int material { get; set; } = 1;
        public int volume { get; set; } = 1000;
        public bool safetyCritical { get; set; } = false;
        public int optimization { get; set; } = 0;
        public int infillType { get; set; } = 0;
        public float infillDensity { get; set; } = 0.3f;
        public int loadCase { get; set; } = 0;
        public float loadForce { get; set; } = 500f;
    }

    public class GenerateResponse
    {
        public string partType { get; set; } = "bracket";
        public float volumeCm3 { get; set; }
        public float weightKg { get; set; }
        public float totalCost { get; set; }
        public float materialCost { get; set; }
        public float processingCost { get; set; }
        public float carbonKg { get; set; }
        public float recyclability { get; set; }
        public float dfmScore { get; set; }
        public float dfaScore { get; set; }
        public float safetyFactor { get; set; }
        public string boxSize { get; set; } = "";
        public System.Collections.Generic.List<string> warnings { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<PrincipleDto> principles { get; set; } = new System.Collections.Generic.List<PrincipleDto>();
    }

    public class PrincipleDto
    {
        public int id { get; set; }
        public string domain { get; set; } = "";
        public string title { get; set; } = "";
        public string desc { get; set; } = "";
        public string impact { get; set; } = "";
    }

    // Custom periodic Diamond lattice SDF pattern implementing PicoGK.IImplicit
    public class ImplicitDiamond : IImplicit
    {
        private float _freqScale;
        private float _thickRatio;

        public ImplicitDiamond(float unitSize, float thicknessRatio)
        {
            _freqScale = (2f * MathF.PI) / unitSize;
            _thickRatio = thicknessRatio;
        }

        public float fSignedDistance(in Vector3 vecPt)
        {
            float x = _freqScale * vecPt.X;
            float y = _freqScale * vecPt.Y;
            float z = _freqScale * vecPt.Z;

            // Diamond surface math model
            float val = MathF.Sin(x) * MathF.Sin(y) * MathF.Sin(z) +
                        MathF.Sin(x) * MathF.Cos(y) * MathF.Cos(z) +
                        MathF.Cos(x) * MathF.Sin(y) * MathF.Cos(z) +
                        MathF.Cos(x) * MathF.Cos(y) * MathF.Sin(z);

            return MathF.Abs(val) - 0.5f * _thickRatio;
        }
    }
}

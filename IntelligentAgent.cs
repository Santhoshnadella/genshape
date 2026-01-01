using System;
using System.Collections.Generic;
using System.Linq;
using PicoGK;
using Leap71.ShapeKernel;

namespace AutomotiveEngineering
{
    public class IntelligentAgent
    {
        private List<string> _chatHistory = new List<string>();
        private DesignInput _currentDesign = new DesignInput();
        private bool _isDesignStarted = false;

        public void StartSession()
        {
            Console.Clear();
            Console.WriteLine("================================================================");
            Console.WriteLine("    AUTOMOTIVE INTELLIGENT ENGINEERING 'ANTIGRAVITY' AGENT      ");
            Console.WriteLine("================================================================");
            Console.WriteLine("Hello! I am your Computational Engineering Assistant.");
            Console.WriteLine("I can help you design automotive components, analyze them against");
            Console.WriteLine("industry standards, and optimize for cost and weight.");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine(KnowledgeBank.GetRandomTip());
            Console.WriteLine("----------------------------------------------------------------");
            
            ChatLoop();
        }

        private void ChatLoop()
        {
            while (true)
            {
                if (!_isDesignStarted)
                {
                    ConversationalDesignInit();
                }
                else
                {
                    Console.Write("\n[You]: ");
                    string input = Console.ReadLine() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(input)) continue;
                    
                    if (input.ToLower().Contains("exit") || input.ToLower().Contains("quit"))
                        break;

                    ProcessInput(input);
                }
            }
        }

        private void ConversationalDesignInit()
        {
            Console.WriteLine("\n[Agent]: Let's start a new design. What component are you looking to build today?");
            Console.WriteLine("(e.g., 'a motor bracket', 'a wheel hub', 'a suspension arm')");
            Console.Write("[You]: ");
            string name = Console.ReadLine() ?? string.Empty;
            _currentDesign.ComponentName = name;

            Console.WriteLine($"\n[Agent]: Understood, we are building a '{name}'.");
            Console.WriteLine("[Agent]: Is this for a high-volume production vehicle (100k+ units) or a prototype/niche vehicle?");
            Console.Write("[You]: ");
            string volStr = (Console.ReadLine() ?? string.Empty).ToLower();
            
            if (volStr.Contains("high") || volStr.Contains("100")) {
                _currentDesign.ProductionVolume = 100000;
                Console.WriteLine("[Agent]: Noted. High volume (100k+). DFM and cycle time will be critical.");
            } else {
                _currentDesign.ProductionVolume = 1000;
                Console.WriteLine("[Agent]: Low volume. tooling costs should be minimized. 3D printing or machining might be viable.");
            }

            Console.WriteLine("\n[Agent]: Is this a safety-critical part? (e.g., suspension, brakes, crash structure)");
            Console.Write("[You]: ");
            string safety = (Console.ReadLine() ?? string.Empty).ToLower();
            _currentDesign.IsSafetyCritical = safety.Contains("yes") || safety.Contains("sure") || safety.Contains("y");
            
            if (_currentDesign.IsSafetyCritical)
                Console.WriteLine("[Agent]: CRITICAL: Will enforce high safety factors and strict compliance checks (ISO 26262).");

            Console.WriteLine($"\n[Agent]: Brief me on the material preference, or say 'recommend' if you're unsure.");
            Console.Write("[You]: ");
            string mat = (Console.ReadLine() ?? string.Empty).ToLower();
            if (mat.Contains("recommend"))
            {
                RecommendMaterial();
            }
            else
            {
                ParseMaterial(mat);
            }

            Console.WriteLine("\n[Agent]: Do you have an existing design proposal? (yes/no)");
            Console.Write("[You]: ");
            string existing = (Console.ReadLine() ?? string.Empty).ToLower();
            if (existing.Contains("yes") || existing.Contains("y"))
            {
                 Console.WriteLine("  > Part Count:");
                 int.TryParse(Console.ReadLine(), out _currentDesign.PartCount);
                 Console.WriteLine("  > Fastener Count:");
                 int.TryParse(Console.ReadLine(), out _currentDesign.FastenerCount);
                 Console.WriteLine("  > Number of Distinct Fastener Types (e.g. M6, M8):");
                 int.TryParse(Console.ReadLine(), out _currentDesign.DistinctFastenerTypes);
                 Console.WriteLine("  > Target Cost ($):");
                 float.TryParse(Console.ReadLine(), out _currentDesign.TargetCost);
            }

            Console.WriteLine("\n[Agent]: Great. I have the basics.");
            Console.WriteLine($"  - Component: {_currentDesign.ComponentName}");
            Console.WriteLine($"  - Volume: {_currentDesign.ProductionVolume}/yr");
            Console.WriteLine($"  - Safety Critical: {_currentDesign.IsSafetyCritical}");
            Console.WriteLine($"  - Material: {_currentDesign.Material}");
            if (_currentDesign.PartCount > 1)
            {
                Console.WriteLine($"  - Design Stats: {_currentDesign.PartCount} parts, {_currentDesign.FastenerCount} fasteners ({_currentDesign.DistinctFastenerTypes} types).");
            }

            Console.WriteLine("\nType 'analyze' to run engineering checks, 'optimize' for suggestions, 'json' for full report, or ask me questions.");
            _isDesignStarted = true;
        }

        private void RecommendMaterial()
        {
            if (_currentDesign.IsSafetyCritical || _currentDesign.ComponentName.Contains("suspension"))
            {
                _currentDesign.Material = MaterialType.Steel; // Default safe
                if (_currentDesign.ProductionVolume < 10000) 
                    Console.WriteLine("[Agent]: Recommendation: High Strength Steel (Machined or Fabricated) for safety and low volume.");
                else
                    Console.WriteLine("[Agent]: Recommendation: Forged Steel or Cast Iron for high durability.");
            }
            else
            {
                _currentDesign.Material = MaterialType.Aluminum;
                Console.WriteLine("[Agent]: Recommendation: Aluminum 6061 for good strength-to-weight ratio.");
            }
        }

        private void ParseMaterial(string input)
        {
            if (input.Contains("steel")) _currentDesign.Material = MaterialType.Steel;
            else if (input.Contains("alum")) _currentDesign.Material = MaterialType.Aluminum;
            else if (input.Contains("plastic")) _currentDesign.Material = MaterialType.Plastic;
            else if (input.Contains("comp") || input.Contains("carbon") || input.Contains("fiber")) _currentDesign.Material = MaterialType.Composite;
            else _currentDesign.Material = MaterialType.Steel; // Default
        }

        private void ProcessInput(string input)
        {
            input = input.ToLower();

            if (input.Contains("analyze"))
            {
                RunAnalysis();
            }
            else if (input.Contains("optimize") || input.Contains("improve"))
            {
                RunOptimization();
            }
            else if (input.Contains("build") || input.Contains("generate"))
            {
                GenerateGeometry();
            }
            else if (input.Contains("cost"))
            {
                ProvideCostEstimate();
            }
            else if (input.Contains("json"))
            {
                GenerateJsonReport();
            }
            else if (input.Contains("help"))
            {
                Console.WriteLine("[Agent]: Commands: 'analyze', 'optimize', 'build', 'cost', 'json', 'reset'.");
            }
            else if (input.Contains("reset"))
            {
                _isDesignStarted = false;
                Console.WriteLine("[Agent]: Resetting session...");
            }
            else
            {
                // Fallback to Knowledge Base Query
                var tip = KnowledgeBank.Principles
                    .OrderBy(x => Guid.NewGuid())
                    .FirstOrDefault(p => input.Contains(p.Title.ToLower()) || input.Contains(p.Domain.ToString().ToLower()));
                
                if (tip != null)
                {
                    Console.WriteLine($"\n[Agent]: Relevant Principle - {tip.Title}");
                    Console.WriteLine($"        {tip.Description}");
                    Console.WriteLine($"        Impact: {tip.Impact}");
                }
                else
                {
                    Console.WriteLine("[Agent]: I can answer questions about DFM, DFA, Stress, etc. or 'analyze' your current design.");
                    Console.WriteLine("        Try asking about 'Structural' or 'Thermal' principles.");
                }
            }
        }

        private void RunAnalysis()
        {
             Console.WriteLine("\n[Agent]: Running Multi-Dimensional Analysis...");
             
             // Create dummy voxels for simulation
             // In a real app we'd scan current geometry.
             var engineer = new EngineeringAssistant();
             
             // We give it a default intended process if not set
             if (_currentDesign.IntendedProcess == 0) 
                _currentDesign.IntendedProcess = _currentDesign.ProductionVolume > 50000 ? ManufacturingProcess.DieCasting : ManufacturingProcess.Machining;

             var report = engineer.AnalyzeDesign(null, _currentDesign);
             
             Console.WriteLine(report.ToString());
             
             if (report.OverallScore < 80)
                Console.WriteLine("\n[Agent]: Score is low. Type 'optimize' to see suggestions.");
        }

        private void RunOptimization()
        {
            Console.WriteLine("\n[Agent]: Searching Knowledge Bank for Optimization Opportunities...");
            
            var suggestions = new List<string>();
            
            // Contextual suggestions based on design state
            if (_currentDesign.ProductionVolume > 10000 && _currentDesign.Material == MaterialType.Aluminum)
                suggestions.Add(KnowledgeBank.Principles.First(p => p.Title == "High Strength Materials").Description);

            suggestions.Add(KnowledgeBank.Principles.First(p => p.Title == "Topology Optimization").Description);
            suggestions.Add(KnowledgeBank.Principles.First(p => p.Title == "Part Consolidation").Description);

            int i = 1;
            foreach(var s in suggestions)
            {
                Console.WriteLine($"{i++}. {s}");
            }
        }

        private void GenerateGeometry()
        {
            Console.WriteLine("\n[Agent]: Interfacing with PicoGK Geometry Kernel...");
            Console.WriteLine("[Agent]: Generating procedural geometry based on parameters...");
            
            try 
            {
                PicoGK.Library.Go(
                0.5f,
                () => {
                    if (_currentDesign.ComponentName.ToLower().Contains("chassis"))
                    {
                        Console.WriteLine("   -> Generative Design: Monocoque Chassis...");
                        var vox = AutomotiveShapes.GenerateMonocoqueChassis_ShapeKernel();
                        Library.oViewer().Add(vox);
                        Sh.ExportVoxelsToSTLFile(vox, Sh.strGetExportPath(Sh.EExport.STL, _currentDesign.ComponentName));
                    }
                    else
                    {
                        Console.WriteLine("   -> Generating Geometry for context...");
                        var sphereShape = new Leap71.ShapeKernel.BaseSphere(new Leap71.ShapeKernel.LocalFrame(), 50f);
                        var vox = sphereShape.voxConstruct();
                        Library.oViewer().Add(vox);
                        Sh.ExportVoxelsToSTLFile(vox, Sh.strGetExportPath(Sh.EExport.STL, _currentDesign.ComponentName));
                    }
                    Console.WriteLine("[Agent]: Viewer launched. Close the viewer window to continue chatting.");
                });
                Console.WriteLine($"[Agent]: SUCCESS. Geometry generated and exported as {_currentDesign.ComponentName}.stl");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Agent]: Error generating geometry: {ex.Message}");
            }
        }

        private void ProvideCostEstimate()
        {
             var engineer = new EngineeringAssistant();
             var costs = engineer.GetType().GetMethod("EstimateCosts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(engineer, new object[] { _currentDesign, 2.5f }) as Dictionary<string, float>;
             
             Console.WriteLine("\n[Agent]: Estimated Cost Breakdown (USD):");
             if (costs != null)
             {
                 foreach(var kvp in costs)
                    Console.WriteLine($"  {kvp.Key}: ${kvp.Value:F2}");
             }
        }

        private void GenerateJsonReport()
        {
             var engineer = new EngineeringAssistant();
             if (_currentDesign.IntendedProcess == 0) _currentDesign.IntendedProcess = ManufacturingProcess.Machining;
             
             var report = engineer.AnalyzeDesign(null, _currentDesign);
             
             string json = report.ToJson();
             Console.WriteLine("\n[Agent]: JSON Report Generated:");
             Console.WriteLine(json);
             
             // System.IO.File.WriteAllText($"{_currentDesign.ComponentName}_report.json", json);
             // Console.WriteLine($"\n[Agent]: Saved to {_currentDesign.ComponentName}_report.json");
        }
    }
}

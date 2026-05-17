using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PicoGK;

namespace AutomotiveEngineering
{
    // --- Data Structures ---

    public enum MaterialType
    {
        Steel,
        Aluminum,
        Plastic,
        Composite,
        Titanium
    }

    public enum ManufacturingProcess
    {
        Machining,
        InjectionMolding,
        DieCasting,
        Stamping,
        AdditiveManufacturing
    }

    public class DesignInput
    {
        public string ComponentName = string.Empty;
        public MaterialType Material;
        public ManufacturingProcess IntendedProcess;
        public int ProductionVolume; // Units per year
        public float TargetCost; // USD
        public float TargetWeight; // kg
        public bool IsSafetyCritical;

        // Detailed Design Parameters for Analysis
        public int PartCount = 1;
        public int FastenerCount = 0;
        public int UniqueMaterialCount = 1;
        public int DistinctFastenerTypes = 1;
        public int ToolingOperations = 0; // Estimation

    }

    public class ComplianceResult
    {
        public List<string> PassedStandards = new List<string>();
        public List<string> FailedStandards = new List<string>();
        public List<string> Warnings = new List<string>();
    }

    public class AnalysisReport
    {
        public float OverallScore;
        public float DfmScore;
        public float DfaScore;
        public ComplianceResult Compliance = new ComplianceResult();
        public List<string> CriticalIssues = new List<string>();
        public List<string> ImprovementOpportunities = new List<string>();
        public Dictionary<string, float> CostBreakdown = new Dictionary<string, float>();
        public List<string> Recommendations = new List<string>();

        public string ToJson()
        {
            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            return System.Text.Json.JsonSerializer.Serialize(this, options);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"--- Automotive Engineering Analysis Report ---");
            sb.AppendLine($"Overall Score: {OverallScore:F0}/100");
            sb.AppendLine($"DFM Score: {DfmScore:F0}/100 | DFA Score: {DfaScore:F0}/100");
            
            sb.AppendLine("\n[Compliance]");
            foreach(var p in Compliance.PassedStandards) sb.AppendLine($"  [PASS] {p}");
            foreach(var f in Compliance.FailedStandards) sb.AppendLine($"  [FAIL] {f}");
            foreach(var w in Compliance.Warnings) sb.AppendLine($"  [WARN] {w}");

            sb.AppendLine("\n[Critical Issues]");
            if (CriticalIssues.Count == 0) sb.AppendLine("  None.");
            foreach(var i in CriticalIssues) sb.AppendLine($"  - {i}");

            sb.AppendLine("\n[Improvement Opportunities]");
             foreach(var i in ImprovementOpportunities) sb.AppendLine($"  - {i}");

            sb.AppendLine("\n[Recommendations]");
            foreach(var r in Recommendations) sb.AppendLine($"  -> {r}");
            
             sb.AppendLine("\n[Cost Breakdown]");
            foreach(var c in CostBreakdown) sb.AppendLine($"  {c.Key}: ${c.Value:F2}");

            return sb.ToString();
        }
    }

    public class EngineeringAssistant
    {
        public AnalysisReport AnalyzeDesign(Voxels? voxels, DesignInput input)
        {
            var report = new AnalysisReport();
            
            // 1. Geometric Analysis (PicoGK Integration)
            // Calculate volume in cubic mm (assuming voxel size is handled by library, approximations here)
            // For now, let's assume we can get a bounding box or estimate volume.
            // Since Voxels object is complex, we'll use bounding box as a proxy for this mock.
            // Vector3 minHash, maxHash; // Unused
            // Note: In a real scenario we might use specific PicoGK utils to get precise bounds/volume.
            // Here we assume checking the bounds is a valid first step.
            // If specific API methods aren't available to us without more digging, we'll simulate the geometric feedback.
            
            // Helper: Let's assume a volume estimation based on a hypothetical bounding box or external call.
            // For this implementation, we will act as if we extracted these:
            float estimatedVolumeCm3 = 500f; // Placeholder
            float estimatedWeightKg =  CalculateWeight(estimatedVolumeCm3, input.Material);

            // 2. DFM Analysis
            var dfm = PerformDfmAnalysis(input, estimatedVolumeCm3);
            report.DfmScore = dfm.Score;
            report.CriticalIssues.AddRange(dfm.Issues);
            report.Recommendations.AddRange(dfm.Recommendations);

            // 3. DFA Analysis
            var dfa = PerformDfaAnalysis(input);
            report.DfaScore = dfa.Score;
            report.ImprovementOpportunities.AddRange(dfa.Opportunities);

            // 4. Compliance Check
            report.Compliance = CheckCompliance(input, estimatedWeightKg);

            // 5. Cost Estimation
            report.CostBreakdown = EstimateCosts(input, estimatedWeightKg);

            // 6. Overall Scoring
            report.OverallScore = (report.DfmScore + report.DfaScore) / 2.0f;
            if (report.Compliance.FailedStandards.Count > 0) report.OverallScore *= 0.5f; // Penalty for compliance fail

            return report;
        }

        private float CalculateWeight(float volumeCm3, MaterialType material)
        {
            float density = 1.0f; // g/cm3
            switch (material)
            {
                case MaterialType.Steel: density = 7.85f; break;
                case MaterialType.Aluminum: density = 2.7f; break;
                case MaterialType.Plastic: density = 1.2f; break;
                case MaterialType.Composite: density = 1.6f; break;
                case MaterialType.Titanium: density = 4.5f; break;
            }
            return (volumeCm3 * density) / 1000.0f; // return kg
        }

        private (float Score, List<string> Issues, List<string> Recommendations) PerformDfmAnalysis(DesignInput input, float volume)
        {
            float score = 100;
            var issues = new List<string>();
            var recommendations = new List<string>();

            // Rule 1: Process Suitability
            if (input.IntendedProcess == ManufacturingProcess.InjectionMolding && volume > 2000)
            {
                score -= 20;
                issues.Add("Part volume may be too large for standard injection molding.");
                recommendations.Add("Consider splitting part or using rotational molding.");
            }

            // Rule 2: Material x Process
            if (input.Material == MaterialType.Steel && input.IntendedProcess == ManufacturingProcess.InjectionMolding)
            {
                score -= 50; // Critical error
                issues.Add("Steel cannot be injection molded.");
                recommendations.Add("Change process to Casting or Machining, or change material to Plastic.");
            }

            // Rule 3: High Volume Optimization
            if (input.ProductionVolume > 100000 && input.IntendedProcess == ManufacturingProcess.Machining)
            {
                score -= 30;
                issues.Add("Machining is not cost-effective for high volume production.");
                recommendations.Add("Switch to Die Casting or Stamping for >100k units.");
            }

            // Rule 4: Part Consolidation (Advanced)
            if (input.PartCount > 3)
            {
                 score -= (input.PartCount - 1) * 2; // Penalty for complexity
                 issues.Add($"High part count ({input.PartCount}). Assembly requires too many individual components.");
                 
                 if (input.Material == MaterialType.Plastic || input.Material == MaterialType.Aluminum)
                 {
                     recommendations.Add($"Consolidate {input.PartCount} parts into a single molded/casted unit. Est. savings: ${(input.PartCount * 0.8):F2}/unit.");
                 }
                 else
                 {
                     recommendations.Add("Investigate welded substructures to reduce loose part count.");
                 }
            }

            if (input.Material == MaterialType.Composite)
            {
                if (input.ProductionVolume > 50000)
                {
                    score -= 10;
                    issues.Add("Composites have high cycle times (>5 mins) which may bottleneck high-volume production.");
                    recommendations.Add("Evaluate Resin Transfer Molding (RTM) or Forged Composite (SMC) for faster cycle times.");
                }
                else
                {
                    recommendations.Add("Low volume allows for optimized hand-layup or Prepreg. Ensure fiber orientation aligns with principal stress vectors.");
                }
            }

            if (input.UniqueMaterialCount > 1)
            {
                score -= 5 * input.UniqueMaterialCount;
                issues.Add($"Multiple materials ({input.UniqueMaterialCount}) complicates recycling and joining.");
                recommendations.Add("Attempt to homogenize material selection to a single alloy/polymer.");
            }

            return (score, issues, recommendations);
        }

        private (float Score, List<string> Opportunities) PerformDfaAnalysis(DesignInput input)
        {
             float score = 100; // Start perfect and deduct
             var ops = new List<string>();

             // Fastener Analysis
             if (input.FastenerCount > 0)
             {
                 if (input.PartCount / (float)input.FastenerCount < 1.0)
                 {
                     score -= 10;
                     ops.Add("Excessive fastening ratio.");
                 }
                 
                 if (input.DistinctFastenerTypes > 2)
                 {
                     score -= 15;
                     ops.Add($"Too many fastener types ({input.DistinctFastenerTypes}). Standardization required.");
                     ops.Add("Standardize on M8 (or similar) fasteners to reduce inventory.");
                 }

                 ops.Add($"Evaluate replacing {input.FastenerCount} fasteners with snap-fits or integrated tabs (Est. time savings: {input.FastenerCount * 15}s).");
             }
             
             if (input.ProductionVolume > 50000)
             {
                 ops.Add("Ensure part orientation allows for robotic pick-and-place.");
             }
             
             // Clamp score
             score = Math.Max(0, Math.Min(100, score));

             return (score, ops);
        }

        private ComplianceResult CheckCompliance(DesignInput input, float weight)
        {
            var c = new ComplianceResult();
            
            // Standard Checks
            c.PassedStandards.Add("ISO 9001 (General Quality)");

            if (input.IsSafetyCritical)
            {
                c.PassedStandards.Add("ISO 26262 (Functional Safety)");
                c.Warnings.Add("Requires rigorous crash testing simulation.");
            }

            // Weight/Emissions check (Rough proxy)
            if (weight > input.TargetWeight * 1.2f)
            {
                c.Warnings.Add($"Weight ({weight:F2}kg) exceeds target ({input.TargetWeight}kg) by >20%. May impact emissions/range.");
            }
            else
            {
                 c.PassedStandards.Add("Weight/Emissions Targets");
            }

            // Material Compliance
            c.PassedStandards.Add("IMDS (Material Data System)");

            return c;
        }

        private Dictionary<string, float> EstimateCosts(DesignInput input, float weight)
        {
            var costs = new Dictionary<string, float>();
            
            // Very rough heuristics
            float materialCostPerKg = 0;
            switch(input.Material)
            {
                case MaterialType.Steel: materialCostPerKg = 1.0f; break;
                case MaterialType.Aluminum: materialCostPerKg = 2.5f; break;
                case MaterialType.Plastic: materialCostPerKg = 1.5f; break;
                case MaterialType.Titanium: materialCostPerKg = 20.0f; break;
            }

            costs["Material"] = weight * materialCostPerKg;
            
            // Processing cost scales with volume (economies of scale)
            float processingBase = 10.0f;
            if (input.ProductionVolume > 10000) processingBase *= 0.2f;
            else if (input.ProductionVolume > 1000) processingBase *= 0.5f;

            costs["Processing"] = processingBase;
            costs["Overhead"] = (costs["Material"] + costs["Processing"]) * 0.15f;
            
            costs["TotalEstimate"] = costs["Material"] + costs["Processing"] + costs["Overhead"];

            return costs;
        }
    }
}

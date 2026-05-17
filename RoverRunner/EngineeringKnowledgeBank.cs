using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomotiveEngineering
{
    public enum EngineeringDomain
    {
        Structural,
        Thermal,
        FluidDynamics,
        Tolerance,
        Cost,
        Weight,
        Manufacturability,
        GeneralDFM,
        GeneralDFA,
        Compliance
    }

    public class EngineeringPrinciple
    {
        public int Id { get; set; }
        public EngineeringDomain Domain { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
    }

    public static class KnowledgeBank
    {
        public static List<EngineeringPrinciple> Principles = new List<EngineeringPrinciple>
        {
            // --- Structural Analysis (1-15) ---
            new EngineeringPrinciple { Id=1, Domain=EngineeringDomain.Structural, Title="Avoid Sharp Corners", Description="Use fillets to reduce stress concentrations.", Impact="Increases fatigue life by up to 50%." },
            new EngineeringPrinciple { Id=2, Domain=EngineeringDomain.Structural, Title="Load Path Continuity", Description="Ensure load paths are direct and continuous to avoid bending moments.", Impact="Reduces weight for required strength." },
            new EngineeringPrinciple { Id=3, Domain=EngineeringDomain.Structural, Title="Section Modulus Optimization", Description="Maximize moment of inertia by placing material away from the neutral axis (e.g., I-beams).", Impact="Maximizes stiffness-to-weight ratio." },
            new EngineeringPrinciple { Id=4, Domain=EngineeringDomain.Structural, Title="Ribbing for Stiffness", Description="Use ribs instead of increasing wall thickness to improve stiffness.", Impact="Saves material and reduces cooling time." },
            new EngineeringPrinciple { Id=5, Domain=EngineeringDomain.Structural, Title="Shear Web Design", Description="Ensure shear webs are properly stabilized against buckling.", Impact="Prevents catastrophic failure in thin-walled structures." },
            new EngineeringPrinciple { Id=6, Domain=EngineeringDomain.Structural, Title="Fatigue Endurance Import", Description="For cyclic loading, design stresses below the endurance limit of the material.", Impact="Ensures infinite life for critical components." },
            new EngineeringPrinciple { Id=7, Domain=EngineeringDomain.Structural, Title="Crash Energy Absorption", Description="Design crumple zones with controlled deformation modes (e.g., accordion folding).", Impact="Vital for passenger safety and compliance." },
            new EngineeringPrinciple { Id=8, Domain=EngineeringDomain.Structural, Title="Torsional Rigidity", Description="Use closed sections (tubes/boxes) rather than open sections (C-channels) for torsion.", Impact="Improves handling and NVH performance." },
            new EngineeringPrinciple { Id=9, Domain=EngineeringDomain.Structural, Title="Hole Placement", Description="Avoid placing holes in high-stress areas; reinforce if necessary.", Impact="Prevents crack initiation." },
            new EngineeringPrinciple { Id=10, Domain=EngineeringDomain.Structural, Title="Material Isotropy", Description="Consider grain direction in rolled metals or fiber orientation in composites.", Impact="Optimizes directional strength." },
            new EngineeringPrinciple { Id=11, Domain=EngineeringDomain.Structural, Title="Buckling Safety Factor", Description="Use higher safety factors for compressive members prone to buckling.", Impact="Prevents sudden structural collapse." },
            new EngineeringPrinciple { Id=12, Domain=EngineeringDomain.Structural, Title="Vibration damping", Description="Design natural frequencies away from excitation frequencies (engine/road).", Impact="Prevents resonance and failure." },
            new EngineeringPrinciple { Id=13, Domain=EngineeringDomain.Structural, Title="Creep Consideration", Description="For high-temp/load, account for plastic deformation over time.", Impact="Maintains dimensional integrity over service life." },
            new EngineeringPrinciple { Id=14, Domain=EngineeringDomain.Structural, Title="Joint Efficiency", Description="Design bolted/welded joints to match the strength of the base material.", Impact="Eliminates weak links in assembly." },
            new EngineeringPrinciple { Id=15, Domain=EngineeringDomain.Structural, Title="Residual Stress Management", Description="Account for stresses from manufacturing (welding/casting) in final analysis.", Impact="Prevents premature warping or failure." },

            // --- Thermal Management (16-30) ---
            new EngineeringPrinciple { Id=16, Domain=EngineeringDomain.Thermal, Title="Surface Area Optimization", Description="Maximize surface area (fins) for convective cooling.", Impact="Improves heat dissipation efficiency." },
            new EngineeringPrinciple { Id=17, Domain=EngineeringDomain.Thermal, Title="Thermal Contact", Description="Ensure high pressure and thermal paste at interfaces to reduce contact resistance.", Impact="Lowers component junction temperatures." },
            new EngineeringPrinciple { Id=18, Domain=EngineeringDomain.Thermal, Title="Match CTEs", Description="Match Coefficients of Thermal Expansion in multimaterial assemblies.", Impact="Prevents thermal stress cracking." },
            new EngineeringPrinciple { Id=19, Domain=EngineeringDomain.Thermal, Title="Convection Paths", Description="Design clear airflow paths for natural or forced convection.", Impact="Reduces fan power requirements." },
            new EngineeringPrinciple { Id=20, Domain=EngineeringDomain.Thermal, Title="Radiation Shielding", Description="Use reflective shields for components near high-heat sources (exhausts).", Impact="Protects sensitive electronics." },
            new EngineeringPrinciple { Id=21, Domain=EngineeringDomain.Thermal, Title="Phase Change Materials", Description="Utilize PCMs for transient heat spikes buffering.", Impact="Stabilizes temperature under variable load." },
            new EngineeringPrinciple { Id=22, Domain=EngineeringDomain.Thermal, Title="Thermal Break", Description="Use insulating washers/gaskets to isolate hot components.", Impact="Protects user-touch points." },
            new EngineeringPrinciple { Id=23, Domain=EngineeringDomain.Thermal, Title="Material Conductivity", Description="Use Aluminum/Copper for spreaders; Plastic/Ceramic for insulators.", Impact="Directs heat flow effectively." },
            new EngineeringPrinciple { Id=24, Domain=EngineeringDomain.Thermal, Title="Active Cooling Triggers", Description="Design control logic to activate fans/pumps only when needed.", Impact="Saves energy and increases life." },
            new EngineeringPrinciple { Id=25, Domain=EngineeringDomain.Thermal, Title="Simulate Hotspots", Description="Use CFD to identify and mitigate local stagnation zones.", Impact="Prevents localized overheating failure." },
            new EngineeringPrinciple { Id=26, Domain=EngineeringDomain.Thermal, Title="Heat Pipe Integration", Description="Use heat pipes to move heat from source to remote sink.", Impact="Enables flexible packaging." },
            new EngineeringPrinciple { Id=27, Domain=EngineeringDomain.Thermal, Title="Thermal Mass", Description="Increase mass for components with short-duration high-power bursts.", Impact="Dampens thermal transients." },
            new EngineeringPrinciple { Id=28, Domain=EngineeringDomain.Thermal, Title="Liquid Cooling Channels", Description="Design conformal cooling channels for uniform temp distribution.", Impact="Increases battery/motor efficiency." },
            new EngineeringPrinciple { Id=29, Domain=EngineeringDomain.Thermal, Title="Radiation Emissivity", Description="Anodize or paint heat sinks black to improve radiative cooling.", Impact="Boosts passive cooling by 10-15%." },
            new EngineeringPrinciple { Id=30, Domain=EngineeringDomain.Thermal, Title="Operating Range Safety", Description="Ensure design accounts for -40C to +85C automotive standard.", Impact="Ensures reliability in all climates." },

            // --- Fluid Dynamics (31-45) ---
            new EngineeringPrinciple { Id=31, Domain=EngineeringDomain.FluidDynamics, Title="Streamlining", Description="Minimize frontal area and improve drag coefficient (Cd).", Impact="Directly improves fuel efficiency/range." },
            new EngineeringPrinciple { Id=32, Domain=EngineeringDomain.FluidDynamics, Title="Laminar Flow Maintenance", Description="Keep surfaces smooth to delay transition to turbulent flow.", Impact="Reduces skin friction drag." },
            new EngineeringPrinciple { Id=33, Domain=EngineeringDomain.FluidDynamics, Title="Bend Radius", Description="Use large radii for pipe/duct bends (R > 1.5D).", Impact="Reduces pressure drop and pump power." },
            new EngineeringPrinciple { Id=34, Domain=EngineeringDomain.FluidDynamics, Title="Diffuser Design", Description="Expand flow cross-sections gradually (< 7 degrees) to avoid separation.", Impact="Recovers pressure efficiently." },
            new EngineeringPrinciple { Id=35, Domain=EngineeringDomain.FluidDynamics, Title="Inlet Design", Description="Place air intakes in high-pressure zones.", Impact="Maximizes mass flow rate." },
            new EngineeringPrinciple { Id=36, Domain=EngineeringDomain.FluidDynamics, Title="Vortex Generators", Description="Use VGs to energize boundary layers and prevent separation.", Impact="Improves lift/downforce control." },
            new EngineeringPrinciple { Id=37, Domain=EngineeringDomain.FluidDynamics, Title="Smooth Transitions", Description="Avoid step changes in flow channels.", Impact="Minimizes turbulence and noise." },
            new EngineeringPrinciple { Id=38, Domain=EngineeringDomain.FluidDynamics, Title="Seal Leakage", Description="Minimize gaps in aerodynamic surfaces.", Impact="Prevents parasitic drag." },
            new EngineeringPrinciple { Id=39, Domain=EngineeringDomain.FluidDynamics, Title="Cooling Drag", Description="Optimize radiator ducting to minimize momentum loss.", Impact="Balances cooling vs drag." },
            new EngineeringPrinciple { Id=40, Domain=EngineeringDomain.FluidDynamics, Title="Underbody Smoothing", Description="Use flat underbody panels.", Impact="Reduces lift and drag significantly." },
            new EngineeringPrinciple { Id=41, Domain=EngineeringDomain.FluidDynamics, Title="Wheel Aerodynamics", Description="Cover wheels or optimize spoke design for pumping losses.", Impact="Reduces overall vehicle drag." },
            new EngineeringPrinciple { Id=42, Domain=EngineeringDomain.FluidDynamics, Title="Mirror Management", Description="Shape mirrors to minimize wake.", Impact="Reduces wind noise and drag." },
            new EngineeringPrinciple { Id=43, Domain=EngineeringDomain.FluidDynamics, Title="Internal Flow Balance", Description="Balance flow between parallel cooling paths.", Impact="Prevents starve-out of critical components." },
            new EngineeringPrinciple { Id=44, Domain=EngineeringDomain.FluidDynamics, Title="Transient Aero", Description="Consider cross-wind stability.", Impact="Improves driver confidence." },
            new EngineeringPrinciple { Id=45, Domain=EngineeringDomain.FluidDynamics, Title="Boat-tailing", Description="Taper rear sections to compress wake.", Impact="Reduces pressure drag." },

            // --- Tolerance Analysis (46-60) ---
            new EngineeringPrinciple { Id=46, Domain=EngineeringDomain.Tolerance, Title="Datums First", Description="Define clear functional datums for manufacturing and measurement.", Impact="Ensures consistent part quality." },
            new EngineeringPrinciple { Id=47, Domain=EngineeringDomain.Tolerance, Title="Worst-Case Stackup", Description="Perform worst-case analysis for safety-critical interferences.", Impact="Guarantees assembly never fails safely." },
            new EngineeringPrinciple { Id=48, Domain=EngineeringDomain.Tolerance, Title="Statistical Stackup (RSS)", Description="Use Root Sum Squares for non-critical fits in high volume.", Impact="Loosens tolerances, reducing cost." },
            new EngineeringPrinciple { Id=49, Domain=EngineeringDomain.Tolerance, Title="MMC/LMC Modifiers", Description="Use Maximum Material Condition circles in GD&T.", Impact="Allows functional gauging and bonus tolerance." },
            new EngineeringPrinciple { Id=50, Domain=EngineeringDomain.Tolerance, Title="Floating Fasteners", Description="Account for clearance hole tolerances in stacks.", Impact="Ensures assembly without binding." },
            new EngineeringPrinciple { Id=51, Domain=EngineeringDomain.Tolerance, Title="Six Sigma Design", Description="Target Cp/Cpk > 1.33 or 1.67.", Impact="Minimizes defect rate to PPM levels." },
            new EngineeringPrinciple { Id=52, Domain=EngineeringDomain.Tolerance, Title="Thermal Expansion Adjustment", Description="Include thermal growth in tolerance stacks.", Impact="Prevents binding at operating temps." },
            new EngineeringPrinciple { Id=53, Domain=EngineeringDomain.Tolerance, Title="Process Capability Match", Description="Don't specify tolerances tighter than process capability.", Impact="Avoids high scrap rates." },
            new EngineeringPrinciple { Id=54, Domain=EngineeringDomain.Tolerance, Title="Slot vs Hole", Description="Use slots for adjustment in one axis.", Impact="Relieves tolerance constraints." },
            new EngineeringPrinciple { Id=55, Domain=EngineeringDomain.Tolerance, Title="3-2-1 Locating", Description="Constrain 6 degrees of freedom explicitly.", Impact="Repeatable positioning." },
            new EngineeringPrinciple { Id=56, Domain=EngineeringDomain.Tolerance, Title="Profile Tolerancing", Description="Use profile of surface for complex curves.", Impact="Controls form and size simultaneously." },
            new EngineeringPrinciple { Id=57, Domain=EngineeringDomain.Tolerance, Title="Zero Tolerance at MMC", Description="Use zero positional tolerance at MMC to maximize acceptance.", Impact="Accepts all functional parts." },
            new EngineeringPrinciple { Id=58, Domain=EngineeringDomain.Tolerance, Title="Datum Target Points", Description="Use points for casting/forging references.", Impact="Avoids rocking on uneven surfaces." },
            new EngineeringPrinciple { Id=59, Domain=EngineeringDomain.Tolerance, Title="Assembly Fixtures", Description="Design features for fixturing, not just function.", Impact="Improves assembly accuracy." },
            new EngineeringPrinciple { Id=60, Domain=EngineeringDomain.Tolerance, Title="Non-Accumulating Dimensions", Description="Dimension from a common baseline, not chain dimensioning.", Impact="Reduces stack-up error." },

            // --- Cost Estimation (61-75) ---
            new EngineeringPrinciple { Id=61, Domain=EngineeringDomain.Cost, Title="Material Utilization", Description="Optimize nesting to reduce offal/scrap.", Impact="Direct material cost reduction." },
            new EngineeringPrinciple { Id=62, Domain=EngineeringDomain.Cost, Title="Standard Stock Sizes", Description="Design to standard plate/bar stick sizes.", Impact="Eliminates custom preparation costs." },
            new EngineeringPrinciple { Id=63, Domain=EngineeringDomain.Cost, Title="Cycle Time Reduction", Description="Design for faster cooling/machining times.", Impact="Increases machine throughput." },
            new EngineeringPrinciple { Id=64, Domain=EngineeringDomain.Cost, Title="Tooling Amortization", Description="Balance tool cost vs piece price (Soft vs Hard tooling).", Impact="Optimizes capital expenditure." },
            new EngineeringPrinciple { Id=65, Domain=EngineeringDomain.Cost, Title="Feature Cost Awareness", Description="Each tolerance/feature adds cost independently.", Impact="Avoids paying for unnecessary precision." },
            new EngineeringPrinciple { Id=66, Domain=EngineeringDomain.Cost, Title="Supply Chain Geography", Description="Source heavy materials locally.", Impact="Reduces logistics costs." },
            new EngineeringPrinciple { Id=67, Domain=EngineeringDomain.Cost, Title="Volume Leverage", Description="Use common parts across platforms.", Impact="Negotiates better volume pricing." },
            new EngineeringPrinciple { Id=68, Domain=EngineeringDomain.Cost, Title="Near-Net Shape", Description="Use casting/forging closest to final shape.", Impact="Reduces machining waste." },
            new EngineeringPrinciple { Id=69, Domain=EngineeringDomain.Cost, Title="Secondary Ops Elimination", Description="Design typically for 'as-molded' finish.", Impact="Removes painting/polishing steps." },
            new EngineeringPrinciple { Id=70, Domain=EngineeringDomain.Cost, Title="Assembly Time", Description="DFA focus directly lowers labor cost.", Impact="Significant per-unit savings." },
            new EngineeringPrinciple { Id=71, Domain=EngineeringDomain.Cost, Title="Quality Cost", Description="Designing out defects is cheaper than inspection.", Impact="Lowers Cost of Poor Quality (COPQ)." },
            new EngineeringPrinciple { Id=72, Domain=EngineeringDomain.Cost, Title="Packaging Efficiency", Description="Design parts to nest for shipping.", Impact="Increases parts per container." },
            new EngineeringPrinciple { Id=73, Domain=EngineeringDomain.Cost, Title="Off-the-shelf vs Custom", Description="COTS is almost always cheaper than Custom.", Impact="Immediate cost and lead time reduction." },
            new EngineeringPrinciple { Id=74, Domain=EngineeringDomain.Cost, Title="Material Grade", Description="Don't over-spec material properties.", Impact="Avoids premium material costs." },
            new EngineeringPrinciple { Id=75, Domain=EngineeringDomain.Cost, Title="Total Landed Cost", Description="Consider duties, shipping, and inventory costs.", Impact="Realistic project budgeting." },

            // --- Weight Optimization (76-90) ---
            new EngineeringPrinciple { Id=76, Domain=EngineeringDomain.Weight, Title="Topology Optimization", Description="Use software to remove unloaded material.", Impact="Can reduce weight by 30-50%." },
            new EngineeringPrinciple { Id=77, Domain=EngineeringDomain.Weight, Title="High Strength Materials", Description="Use HSS (High Strength Steel) to thin down walls.", Impact="Maintains strength with less mass." },
            new EngineeringPrinciple { Id=78, Domain=EngineeringDomain.Weight, Title="Composites Integration", Description="Replace metal with CFRP or GFRP where appropriate.", Impact="Significant weight drop." },
            new EngineeringPrinciple { Id=79, Domain=EngineeringDomain.Weight, Title="Hollow Structures", Description="Use tubes/hollow, not solids.", Impact="High stiffness/mass ratio." },
            new EngineeringPrinciple { Id=80, Domain=EngineeringDomain.Weight, Title="Part Consolidation", Description="Removing fasteners and flanges saves weight.", Impact="Eliminates parasitic mass." },
            new EngineeringPrinciple { Id=81, Domain=EngineeringDomain.Weight, Title="Mass De-compounding", Description="Primary weight cut allows lighter suspension/brakes.", Impact="Virtuous cycle of weight reduction." },
            new EngineeringPrinciple { Id=82, Domain=EngineeringDomain.Weight, Title="Magnesium/Aluminum", Description="Switch from steel to lighter alloys.", Impact="50-75% density reduction." },
            new EngineeringPrinciple { Id=83, Domain=EngineeringDomain.Weight, Title="Sandwich Panels", Description="Use honeycomb cores with thin skins.", Impact="Extreme stiffness for minimal weight." },
            new EngineeringPrinciple { Id=84, Domain=EngineeringDomain.Weight, Title="Lightweight Fasteners", Description="Use Aluminum or Titanium bolts.", Impact="Reduces assembly weight." },
            new EngineeringPrinciple { Id=85, Domain=EngineeringDomain.Weight, Title="Laser Tailored Blanks", Description="Weld variable thickness sheets before stamping.", Impact="Puts metal only where needed." },
            new EngineeringPrinciple { Id=86, Domain=EngineeringDomain.Weight, Title="Foam Injection", Description="Use MuCell or structural foam.", Impact="Reduces density of plastic parts." },
            new EngineeringPrinciple { Id=87, Domain=EngineeringDomain.Weight, Title="Wire Harness Optimization", Description="Multiplexing reduces copper weight.", Impact="Saves several kg per vehicle." },
            new EngineeringPrinciple { Id=88, Domain=EngineeringDomain.Weight, Title="Glazing", Description="Use polycarbonate or thinner glass.", Impact="Lowers center of gravity." },
            new EngineeringPrinciple { Id=89, Domain=EngineeringDomain.Weight, Title="Unsprung Mass focus", Description="Prioritize wheels/brakes weight reduction.", Impact="Improves ride and handling disproportionately." },
            new EngineeringPrinciple { Id=90, Domain=EngineeringDomain.Weight, Title="Function Integration", Description="Use body structure as battery enclosure.", Impact="Eliminates redundant structures." },

            // --- Manufacturability Scoring (91-105) ---
            new EngineeringPrinciple { Id=91, Domain=EngineeringDomain.Manufacturability, Title="Draft Angles", Description="Ensure all molded/cast parts have draft.", Impact="Essential for part ejection." },
            new EngineeringPrinciple { Id=92, Domain=EngineeringDomain.Manufacturability, Title="Undercut Avoidance", Description="Avoid geometry requiring side-actions/slides.", Impact="Reduces tool cost and complexity." },
            new EngineeringPrinciple { Id=93, Domain=EngineeringDomain.Manufacturability, Title="Uniform Wall Thickness", Description="Maintain constant wall thickness to avoid sinks/warping.", Impact="Improves cosmetic and structural quality." },
            new EngineeringPrinciple { Id=94, Domain=EngineeringDomain.Manufacturability, Title="Corner Radii", Description="Avoid sharp internal corners in machining.", Impact="Allows standard cutter usage." },
            new EngineeringPrinciple { Id=95, Domain=EngineeringDomain.Manufacturability, Title="Access for Tooling", Description="Ensure space for wrenches/drivers.", Impact="Enables assembly." },
            new EngineeringPrinciple { Id=96, Domain=EngineeringDomain.Manufacturability, Title="Self-Locating Parts", Description="Features should align themselves.", Impact="Reduces jigs usage." },
            new EngineeringPrinciple { Id=97, Domain=EngineeringDomain.Manufacturability, Title="Symmetry/Asymmetry", Description="Make parts obviously symmetric or effectively asymmetric.", Impact="Prevents backward installation." },
            new EngineeringPrinciple { Id=98, Domain=EngineeringDomain.Manufacturability, Title="Min Part Size", Description="Avoid parts too small to handle.", Impact="Improves ergonomic handling." },
            new EngineeringPrinciple { Id=99, Domain=EngineeringDomain.Manufacturability, Title="Gravitational Stability", Description="Parts should be stable when placed before fastening.", Impact="Eases manual assembly." },
            new EngineeringPrinciple { Id=100, Domain=EngineeringDomain.Manufacturability, Title="Standard Fasteners", Description="Use common drive types and sizes.", Impact="Reduces tool changeover." },
            new EngineeringPrinciple { Id=101, Domain=EngineeringDomain.Manufacturability, Title="Tolerance allocations", Description="Loose tolerances where possible.", Impact="Increases yield." }
        };

        public static List<EngineeringPrinciple> GetDomainPrinciples(EngineeringDomain domain)
        {
            return Principles.Where(p => p.Domain == domain).ToList();
        }

        public static string GetRandomTip()
        {
            var rand = new Random();
            var p = Principles[rand.Next(Principles.Count)];
            return $"Did you know? {p.Title}: {p.Description} ({p.Impact})";
        }
    }
}

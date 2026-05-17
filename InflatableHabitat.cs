using System;
using System.Numerics;
using PicoGK;

namespace Leap71.HabitatExample
{
    public class InflatableHabitat
    {
        // Habitat Parameters
        float m_fStowedRadius = 30f; // Slightly wider when flat?
        float m_fDeployedRadius = 50f;
        float m_fStowedHeight = 5f;
        float m_fDeployedHeight = 60f;
        float m_fWallThickness = 2f;

        int m_nTiers = 5;
        int m_nSegments = 12; // Faceted look

        // Deployment state: 0.0 = Stowed, 1.0 = Deployed
        float m_fDeployment = 1.0f;

        public InflatableHabitat(float fDeployment)
        {
            m_fDeployment = Math.Clamp(fDeployment, 0f, 1f);
        }

        float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public Voxels voxConstruct()
        {
            // 1. Generate the Faceted Outer Mesh
            Mesh mshOuter = mshCreateOrigamiDome(m_fDeployment);
            
            // 2. Convert to Voxels
            Voxels voxShape = new Voxels(mshOuter);

            // 3. Create Hollow Shell (inward offset)
            // voxShell with negative offset keeps the outer surface and creates a void inside
            Voxels voxHabitat = voxShape.voxShell(-m_fWallThickness);

            // 4. Cutout Doorway
            Voxels voxDoor = voxCreateDoor();
            voxHabitat -= voxDoor;

            return voxHabitat;
        }

        Mesh mshCreateOrigamiDome(float fDeploy)
        {
            Mesh msh = new Mesh();
            float fCurrentHeight = Lerp(m_fStowedHeight, m_fDeployedHeight, fDeploy);
            
            // Generate Rings
            List<List<int>> lRingIndices = new List<List<int>>();

            // We build tiers. 
            // Tier 0 is floor (Z=0). Tier N is top (Z=Height).
            for (int i = 0; i <= m_nTiers; i++)
            {
                float fH_ratio = (float)i / (float)m_nTiers;
                float fZ = fH_ratio * fCurrentHeight;
                
                // Profile Logic (Dome/Beehive shape)
                // Sine wave profile: sin(0) -> sin(PI)
                // We want it to start with straight walls maybe?
                // Let's use a Superellipse or just a simple curve.
                // Radius expands then contracts.
                
                // 0.0 -> 1.0 (equator) -> 0.0 (top)?
                // Let's do a truncated shape. Bottom is wide, Top is narrower.
                // Base Radius -> Top Radius.
                // Using a sine curve for a nice bulging habitat look as per images.
                // Angle from 0 to PI/2? or PI?
                // Image e shows a "tapered" top.
                
                float fAngle = fH_ratio * (MathF.PI * 0.7f); // 0 to ~126 degrees
                float fProfileScale = MathF.Cos(fAngle - 0.5f); // Shifted curve
                // Simple Taper:
                float fTaper = 1.0f - (fH_ratio * 0.6f); // Top is 40% of bottom
                
                // Combine Deployment expansion?
                // When stowed (fDeploy=0), radius is m_fStowedRadius.
                // When deployed (fDeploy=1), radius is m_fDeployedRadius * fTaper.
                float fRadBase = Lerp(m_fStowedRadius, m_fDeployedRadius, fDeploy);
                float fR = fRadBase * fTaper; 

                // Origami "ZigZag" offset
                // odd rings rotated by half segment?
                float fRotOffset = (i % 2 == 0) ? 0f : (MathF.PI / m_nSegments);

                List<int> lCurrentRing = new List<int>();

                for (int j = 0; j < m_nSegments; j++)
                {
                    float fPhi = (2f * MathF.PI * j) / m_nSegments + fRotOffset;
                    float fX = fR * MathF.Cos(fPhi);
                    float fY = fR * MathF.Sin(fPhi);
                    
                    lCurrentRing.Add(msh.nAddVertex(new Vector3(fX, fY, fZ)));
                }
                lRingIndices.Add(lCurrentRing);
            }

            // Cap Bottom
            int nCenterBottom = msh.nAddVertex(new Vector3(0, 0, 0));
            // Create fan
            for (int j = 0; j < m_nSegments; j++)
            {
                int nNext = (j + 1) % m_nSegments;
                // Bottom face needs to point Down. Normal (v1-v0)x(v2-v0). 
                // Center(0), Ring(current), Ring(next). 
                // Z=0.
                msh.nAddTriangle(nCenterBottom, lRingIndices[0][nNext], lRingIndices[0][j]);
            }

            // Build Sides (Triangulated strips)
            for (int i = 0; i < m_nTiers; i++)
            {
                var lBottom = lRingIndices[i];
                var lTop = lRingIndices[i+1];

                for (int j = 0; j < m_nSegments; j++)
                {
                    int nNext = (j + 1) % m_nSegments;
                    
                    // Quad: B_j, B_next, T_next, T_j
                    // Tri 1: B_j, B_next, T_j
                    // Tri 2: B_next, T_next, T_j
                    // Orientation: Counter Clockwise from outside
                    
                    msh.nAddTriangle(lBottom[j], lBottom[nNext], lTop[j]);
                    msh.nAddTriangle(lBottom[nNext], lTop[nNext], lTop[j]);
                }
            }

            // Cap Top
            int nCenterTop = msh.nAddVertex(new Vector3(0, 0, fCurrentHeight));
            for (int j = 0; j < m_nSegments; j++)
            {
                int nNext = (j + 1) % m_nSegments;
                // Top face points Up.
                msh.nAddTriangle(nCenterTop, lRingIndices[m_nTiers][j], lRingIndices[m_nTiers][nNext]);
            }

            return msh;
        }

        Voxels voxCreateDoor()
        {
            // Create a box for the door
            // Positioned at X-axis boundary, z from floor to mid-height
            float fDoorWidth = 15f;
            float fDoorHeight = 25f;
            float fDepth = 20f; // Through the wall
            
            // Center of door at (Radius, 0, Height/2) roughly
            Vector3 vecSize = new Vector3(fDepth, fDoorWidth, fDoorHeight);
            Vector3 vecCenter = new Vector3(m_fDeployedRadius, 0, fDoorHeight/2f + 2f); // +2 for floor offset
            
            return new Voxels(PicoGK.Utils.mshCreateCube(vecSize, vecCenter));
        }
    }
}

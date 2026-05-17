using System;
using System.Numerics;
using PicoGK;

namespace Leap71.FusorExample
{
    public class Fusor
    {
        // Fusor Parameters
        float m_fChamOuterRad = 80f;
        float m_fChamWallThick = 5f;
        float m_fOuterGridRad = 50f;
        float m_fInnerGridRad = 15f;
        float m_fPortRad = 15f;
        float m_fPortLength = 40f;

        public Voxels voxConstruct()
        {
            // 1. Create the Vacuum Chamber (Hollow Sphere with Ports)
            Voxels voxChamber = voxCreateChamber();

            // 2. Create Outer Grid (Anode)
            Voxels voxOuterGrid = voxCreateGrid(m_fOuterGridRad, 1.5f);

            // 3. Create Inner Grid (Cathode)
            Voxels voxInnerGrid = voxCreateGrid(m_fInnerGridRad, 1.0f);

            // Combine
            Voxels voxResult = voxChamber;
            voxResult += voxOuterGrid;
            voxResult += voxInnerGrid;

            return voxResult;
        }

        Voxels voxCreateChamber()
        {
            // Base Sphere
            Voxels voxCham = new Voxels(PicoGK.Utils.mshCreateGeoSphere(new Vector3(m_fChamOuterRad * 2f)));
            
            // Subtract Inner Sphere to make it hollow
            Voxels voxInner = new Voxels(PicoGK.Utils.mshCreateGeoSphere(new Vector3((m_fChamOuterRad - m_fChamWallThick) * 2f)));
            voxCham -= voxInner;

            // Add Ports (Cylinders)
            // Top Port
            Voxels voxTopPort = new Voxels(PicoGK.Utils.mshCreateCylinder(new Vector3(m_fPortRad * 2f, m_fPortRad * 2f, m_fPortLength), new Vector3(0, 0, m_fChamOuterRad - 2f)));
            // Bottom Port
             // PicoGK Cylinder is defined by size (bounding box logic roughly) and offset. 
             // mshCreateCylinder(size, offset) -> cylinder is Z aligned.
             // We need to rotate for side ports or creating them in place.
             // Let's create one generic cylinder and transform it.
            
            Voxels voxBotPort = new Voxels(MakeCylinderMesh(m_fPortRad, m_fPortLength, new Vector3(0, 0, -(m_fChamOuterRad - 2f) - m_fPortLength))); // Manual negative Z
            
            // Side Port - needing rotation.
            Mesh mshSide = MakeCylinderMesh(m_fPortRad, m_fPortLength, Vector3.Zero);
            // Rotate 90 deg around Y to point in X
            mshSide = mshSide.mshCreateTransformed(Matrix4x4.CreateRotationY(MathF.PI / 2f));
            // Move to X
            mshSide = mshSide.mshCreateTransformed(Matrix4x4.CreateTranslation(m_fChamOuterRad - 2f, 0, 0));
            Voxels voxSidePort = new Voxels(mshSide);

            // Hollow out ports (Subtract cylinder with slightly smaller radius)
            // Top Hole
            Voxels voxTopHole = new Voxels(PicoGK.Utils.mshCreateCylinder(new Vector3((m_fPortRad - 3f) * 2f, (m_fPortRad - 3f) * 2f, m_fPortLength + 20), new Vector3(0, 0, m_fChamOuterRad - 10f)));
            
            // Bot Hole
            Voxels voxBotHole = new Voxels(MakeCylinderMesh(m_fPortRad - 3f, m_fPortLength + 20, new Vector3(0, 0, -(m_fChamOuterRad - 10f) - (m_fPortLength + 20))));

            // Side Hole
            Mesh mshSideHole = MakeCylinderMesh(m_fPortRad - 3f, m_fPortLength + 20, Vector3.Zero);
            mshSideHole = mshSideHole.mshCreateTransformed(Matrix4x4.CreateRotationY(MathF.PI / 2f));
            mshSideHole = mshSideHole.mshCreateTransformed(Matrix4x4.CreateTranslation(m_fChamOuterRad - 10f, 0, 0));
            Voxels voxSideHole = new Voxels(mshSideHole);

            // Cut holes through the main chamber wall at port locations
            voxCham -= voxTopHole;
            voxCham -= voxBotHole;
            voxCham -= voxSideHole;

            // Add the port tubes
            Voxels voxPorts = voxTopPort;
            voxPorts += voxBotPort;
            voxPorts += voxSidePort;
            
            // Hollow ports
            voxPorts -= voxTopHole;
            voxPorts -= voxBotHole;
            voxPorts -= voxSideHole;

            voxCham += voxPorts;
            return voxCham;
        }

        Mesh MakeCylinderMesh(float fRadius, float fLength, Vector3 vecOffset)
        {
             // PicoGK mshCreateCylinder size Z is the height. width/depth is diameter.
             return PicoGK.Utils.mshCreateCylinder(new Vector3(fRadius * 2f, fRadius * 2f, fLength), vecOffset);
        }

        Voxels voxCreateGrid(float fRadius, float fBeamThick)
        {
            // Create a geodesic-like wireframe using a Lattice
            Lattice latGrid = new Lattice();

            // Simple latitude wires
            int nLatitudes = 6; 
            for (int i = 0; i < nLatitudes; i++)
            {
                // Create rings around Z axis
                // Using 360 points for smoothness
                List<Vector3> aPoints = new List<Vector3>();
                float fTheta = (MathF.PI * (i + 1)) / (nLatitudes + 1); // Avoid poles strictly
                float fRingRad = fRadius * MathF.Sin(fTheta);
                float fRingZ = fRadius * MathF.Cos(fTheta);

                for (int j = 0; j <= 60; j++)
                {
                   float fPhi = (2f * MathF.PI * j) / 60f;
                   aPoints.Add(new Vector3(fRingRad * MathF.Cos(fPhi), fRingRad * MathF.Sin(fPhi), fRingZ));
                }
                for (int k = 0; k < aPoints.Count - 1; k++)
                {
                    latGrid.AddBeam(aPoints[k], aPoints[k+1], fBeamThick, fBeamThick);
                }
            }

            // Longitude wires (Meridians)
            int nLongitudes = 8;
            for (int i = 0; i < nLongitudes; i++)
            {
                float fPhi = (MathF.PI * 2f * i) / nLongitudes;
                List<Vector3> aPoints = new List<Vector3>();
                for (int j = 0; j <= 60; j++)
                {
                    float fTheta = (MathF.PI * 2f * j) / 60f; // Full circle
                    Vector3 p = new Vector3(fRadius * MathF.Sin(fTheta), 0, fRadius * MathF.Cos(fTheta));
                    // Rotate around Z by Phi
                    float fX = p.X * MathF.Cos(fPhi) - p.Y * MathF.Sin(fPhi);
                    float fY = p.X * MathF.Sin(fPhi) + p.Y * MathF.Cos(fPhi);
                    aPoints.Add(new Vector3(fX, fY, p.Z));
                }
                
                for (int k = 0; k < aPoints.Count - 1; k++)
                {
                    latGrid.AddBeam(aPoints[k], aPoints[k+1], fBeamThick, fBeamThick);
                }
            }
            
            Voxels vox = new Voxels();
            vox.RenderLattice(latGrid);
            return vox;
        }
    }
}

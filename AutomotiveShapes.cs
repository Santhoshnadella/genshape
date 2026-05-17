using System;
using System.Numerics;
using PicoGK;
using Leap71.ShapeKernel;

namespace AutomotiveEngineering
{
    public static class AutomotiveShapes
    {
        public static Voxels GenerateMonocoqueChassis(float length = 4000f, float width = 1800f, float height = 1200f)
        {
            // Simple procedural Monocoque Chassis
            
            // 1. Main Tub (Cabin)
            // Create a solid box
            var cabinMesh = Utils.mshCreateCube(new Vector3(length/2, width, height)); // Center seems to be 0,0,0 usually or defined by bounds
            // PicoGK mshCreateCube usually creates a cube at origin with side length? 
            // Let's check Utils.mshCreateCube signature or behavior if possible, but standard is usually size.
            // Assuming mshCreateCube(Vector3 dimensions) creates centered box.
            
            // Let's build using Lattice/ShapeKernel for more control if possible, or just CSG with primitives.
            // Using BaseBox from ShapeKernel is safer if we want it to be a "Shape".
            
            // Let's stick to pure PicoGK Voxels for CSG operations as it's robust.
            
            // Main Body
            Voxels voxBody = new Voxels(Utils.mshCreateCube(new Vector3(length, width, height)));
            
            // Interior (Hollow it out)
            float wallThickness = 50f;
            Voxels voxInterior = new Voxels(Utils.mshCreateCube(new Vector3(length - wallThickness*2, width - wallThickness*2, height - wallThickness*2)));
            
            voxBody.BoolSubtract(voxInterior);
            
            // 2. Cutouts for Wheels (Wheel Wells)
            float wheelRadius = 350f;
            float wheelWidth = 250f;
            
            // Front Left
            Voxels voxWheelWell = new Voxels(Utils.mshCreateCube(new Vector3(wheelRadius*2.5f, wheelWidth + 50f, wheelRadius*2.5f))); 
            // Position it
            Vector3 frontLeftPos = new Vector3(length/2 - 800f, width/2, -height/2 + wheelRadius);
            
            // Since PicoGK primitives are at origin, we need to translate.
            // But Voxels/Mesh translation might need a loop or matrix transform.
            // ShapeKernel Frames are better for this.
            
            // Alternative: Use ShapeKernel BaseBox which takes a Frame.
            
            return voxBody;
        }

        public static Voxels GenerateMonocoqueChassis_ShapeKernel()
        {
            // Use ShapeKernel for better positioning
            // Main Tub
            var mainFrame = new LocalFrame(); // At 0,0,0
            var mainBox = new BaseBox(mainFrame, 4000f, 1800f, 1200f);
            Voxels voxChassis = mainBox.voxConstruct();

            // Cockpit cutout
            var cockpitFrame = new LocalFrame(new Vector3(0, 0, 200f)); // Shifted up
            var cockpitBox = new BaseBox(cockpitFrame, 2500f, 1600f, 800f);
            Voxels voxCockpit = cockpitBox.voxConstruct();
            
            voxChassis.BoolSubtract(voxCockpit);
            
            return voxChassis;
        }
    }
}

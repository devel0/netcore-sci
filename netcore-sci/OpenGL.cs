using System.Numerics;
using System.Runtime.InteropServices;
using static System.Math;

namespace SearchAThing.Sci
{


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GLLineVertex
    {
        public Vector3 Position;
        //public Vector3 Normal;
    }
    
    public static partial class SciExt
    {

        public static GLLineVertex ToGLLineVertex(this netDxf.Vector3 vector)
        {
            return new GLLineVertex
            {
                Position = new Vector3((float)vector.X, (float)vector.Y, (float)vector.Z)
            };
        }

    }



}
using System.Numerics;
using System.Runtime.InteropServices;

namespace SearchAThing
{


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GLLineVertex
    {
        public Vector3 Position;
        //public Vector3 Normal;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GLTriangleVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
    }

    public static partial class SciExt
    {

        public static GLTriangleVertex ToGLTriangleVertex(this Vector3D v)
        {
            return new GLTriangleVertex
            {
                Position = new Vector3((float)v.X, (float)v.Y, (float)v.Z)
            };
        }

        public static GLLineVertex ToGLLineVertex(this Vector3D v)
        {
            return new GLLineVertex
            {
                Position = new Vector3((float)v.X, (float)v.Y, (float)v.Z)
            };
        }

        public static GLLineVertex ToGLLineVertex(this netDxf.Vector3 v)
        {
            return new GLLineVertex
            {
                Position = new Vector3((float)v.X, (float)v.Y, (float)v.Z)
            };
        }

    }

}
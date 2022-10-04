using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Math;

namespace SearchAThing
{

    public static partial class SciExt
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Transform(this Vector4 v, Matrix4x4 m) =>
            Vector4.Transform(v, m);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Normalized(this Vector4 v) => Vector4.Normalize(v);

        /// <summary>
        /// convert given Vector4 to Vector3D ( discarding w )
        /// </summary>
        /// <param name="v">Vector4</param>
        /// <returns>Vector3D</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D ToVector3D(this Vector4 v) => new Vector3D(v.X, v.Y, v.Z);

        /// <summary>
        /// create vector3 from vector4 discarding w
        /// </summary>
        /// <param name="v">vector4 input</param>
        /// <returns>vector3</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3(this Vector4 v) => new Vector3(v.X, v.Y, v.Z);

         /// <summary>
        /// create vector2 from vector4 discarding z, w
        /// </summary>
        /// <param name="v">vector4 input</param>
        /// <returns>vector2</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToVector2(this Vector4 v) => new Vector2(v.X, v.Y);

        /// <summary>
        /// debug to console with optional prefix
        /// </summary>
        /// <param name="v">vector</param>
        /// <param name="prefix">optional prefix</param>
        /// <returns>vector</returns>
        public static Vector4 Debug(this Vector4 v, string prefix = "")
        {
            System.Diagnostics.Debug.WriteLine($"{(prefix.Length > 0 ? ($"{prefix}:") : "")}{v}");
            return v;
        }

    }

}
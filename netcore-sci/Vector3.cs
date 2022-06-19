using System.Numerics;
using System.Runtime.CompilerServices;

namespace SearchAThing
{

    public static partial class SciExt
    {

        /// <summary>
        /// return clamped Vector3 between [min,max] interval
        /// </summary>
        /// <param name="v">xyz vector</param>
        /// <param name="min">min value admissible</param>
        /// <param name="max">max value admissible</param>
        /// <returns>given vector with xyz components clamped to corresponding min,max components</returns>        
        public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max)
        {
            var vx = v.X.Clamp(min.X, max.X);
            var vy = v.Y.Clamp(min.Y, max.Y);
            var vz = v.Z.Clamp(min.Z, max.Z);

            return new Vector3(vx, vy, vz);
        }

        /// <summary>
        /// debug to console with optional prefix
        /// </summary>
        /// <param name="v">vector</param>
        /// <param name="prefix">optional prefix</param>
        /// <returns>vector</returns>
        public static Vector3 Debug(this Vector3 v, string prefix = "")
        {
            System.Diagnostics.Debug.WriteLine($"{(prefix.Length > 0 ? ($"{prefix}:") : "")}{v}");
            return v;
        }

        /// <summary>
        /// normalize given vector
        /// </summary>
        /// <param name="v">vector</param>
        /// <returns>normalized vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalized(this Vector3 v) => Vector3.Normalize(v);

        /// <summary>
        /// convert given vector3 to vector3d
        /// </summary>
        /// <param name="v">vector3</param>
        /// <returns>vector3d</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D ToVector3D(this Vector3 v) => (Vector3D)v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Transform(this Vector3 v, Matrix4x4 m) => Vector3.Transform(v, m);

        /// <summary>
        /// create new Vector3 with given x overriden, others yz unchanged
        /// </summary>
        /// <param name="v">vector</param>
        /// <param name="x">x value to set</param>
        /// <returns>changed vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetX(this Vector3 v, float x) => new Vector3(x, v.Y, v.Z);

        /// <summary>
        /// create new Vector3 with given y overriden, others xz unchanged
        /// </summary>
        /// <param name="v">vector</param>
        /// <param name="y">y value to set</param>
        /// <returns>changed vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetY(this Vector3 v, float y) => new Vector3(v.X, y, v.Z);

        /// <summary>
        /// create new Vector3 with given z overriden, others xy unchanged
        /// </summary>
        /// <param name="v">vector</param>
        /// <param name="z">z value to set</param>
        /// <returns>changed vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetZ(this Vector3 v, float z) => new Vector3(v.X, v.Y, z);

        /// <summary>
        /// convert given vector to degrees
        /// </summary>
        /// <param name="v">vector source ( radians )</param>
        /// <returns>vector ( degrees )</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToDeg(this Vector3 v) => new Vector3(v.X.ToDeg(), v.Y.ToDeg(), v.Z.ToDeg());

        /// <summary>
        /// convert given vector to radians
        /// </summary>
        /// <param name="v">vector source ( degrees )</param>
        /// <returns>vector ( radians )</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToRad(this Vector3 v) => new Vector3(v.X.ToRad(), v.Y.ToRad(), v.Z.ToRad());

    }

}
using System.Runtime.CompilerServices;

using NVector3 = System.Numerics.Vector3;
using DVector3 = netDxf.Vector3;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace SearchAThing
{

	public static partial class SciExt
    {

        /// <summary>
        /// convert System.Numerics.Vector3 to netdxf.Vector3
        /// </summary>        
        public static DVector3 ToDVector3(this NVector3 v) => new DVector3(v.X, v.Y, v.Z);

        /// <summary>
        /// convert netdxf.Vector3 to System.Numerics.Vector3 ( double cast to float )
        /// </summary>
        public static NVector3 ToNVector3(this DVector3 v) => new NVector3((float)v.X, (float)v.Y, (float)v.Z);

        /// <summary>
        /// return clamped Vector3 between [min,max] interval
        /// </summary>
        /// <param name="v">xyz vector</param>
        /// <param name="min">min value admissible</param>
        /// <param name="max">max value admissible</param>
        /// <returns>given vector with xyz components clamped to corresponding min,max components</returns>        
        public static NVector3 Clamp(this NVector3 v, NVector3 min, NVector3 max)
        {
            var vx = v.X.Clamp(min.X, max.X);
            var vy = v.Y.Clamp(min.Y, max.Y);
            var vz = v.Z.Clamp(min.Z, max.Z);

            return new NVector3(vx, vy, vz);
        }

        /// <summary>
        /// return abs of given Vector3
        /// </summary>        
        public static NVector3 Abs(this NVector3 v) => new NVector3(v.X, v.Y, v.Z);

        /// <summary>
        /// debug to console with optional prefix
        /// </summary>
        /// <param name="v">vector</param>
        /// <param name="prefix">optional prefix</param>
        /// <returns>vector</returns>
        public static NVector3 Debug(this NVector3 v, string prefix = "")
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
        public static NVector3 Normalized(this NVector3 v) => NVector3.Normalize(v);

        /// <summary>
        /// convert given vector3 to vector3d
        /// </summary>
        /// <param name="v">vector3</param>
        /// <returns>vector3d</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D ToVector3D(this NVector3 v) => (Vector3D)v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NVector3 Transform(this NVector3 v, Matrix4x4 m) => NVector3.Transform(v, m);

        /// <summary>
        /// create new Vector3 with given x overriden, others yz unchanged
        /// </summary>
        /// <param name="v">vector</param>
        /// <param name="x">x value to set</param>
        /// <returns>changed vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NVector3 SetX(this NVector3 v, float x) => new NVector3(x, v.Y, v.Z);

        /// <summary>
        /// create new Vector3 with given y overriden, others xz unchanged
        /// </summary>
        /// <param name="v">vector</param>
        /// <param name="y">y value to set</param>
        /// <returns>changed vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NVector3 SetY(this NVector3 v, float y) => new NVector3(v.X, y, v.Z);

        /// <summary>
        /// create new Vector3 with given z overriden, others xy unchanged
        /// </summary>
        /// <param name="v">vector</param>
        /// <param name="z">z value to set</param>
        /// <returns>changed vector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NVector3 SetZ(this NVector3 v, float z) => new NVector3(v.X, v.Y, z);

        /// <summary>
        /// convert given vector to degrees
        /// </summary>
        /// <param name="v">vector source ( radians )</param>
        /// <returns>vector ( degrees )</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NVector3 ToDeg(this NVector3 v) => new NVector3(v.X.ToDeg(), v.Y.ToDeg(), v.Z.ToDeg());

        /// <summary>
        /// convert given vector to radians
        /// </summary>
        /// <param name="v">vector source ( degrees )</param>
        /// <returns>vector ( radians )</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NVector3 ToRad(this NVector3 v) => new NVector3(v.X.ToRad(), v.Y.ToRad(), v.Z.ToRad());

        public static bool EqualsTol(this NVector3 v, double tol, NVector3 other) =>
            ((double)v.X).EqualsTol(tol, other.X) &&
            ((double)v.Y).EqualsTol(tol, other.Y) &&
            ((double)v.Z).EqualsTol(tol, other.Z);

    }

    /// <summary>
    /// helper class to compare vector3d set using given tolerance
    /// </summary>
    public class NVector3EqualityComparer : IEqualityComparer<NVector3>
    {
        double tol;

        public NVector3EqualityComparer(double _tol)
        {
            tol = _tol;
        }

        public bool Equals(NVector3 a, NVector3 b) => a.EqualsTol(tol, b);

        public int GetHashCode(NVector3 obj) => 0;
    }

    /// <summary>
    /// helper class to compare vector3d set using given tolerance
    /// </summary>
    public class DVector3EqualityComparer : IEqualityComparer<DVector3>
    {
        double tol;

        public DVector3EqualityComparer(double _tol)
        {
            tol = _tol;
        }

        public bool Equals(DVector3 a, DVector3 b) => a.EqualsTol(tol, b);

        public int GetHashCode(DVector3 obj) => 0;
    }

}
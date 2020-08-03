using System.Numerics;

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

    }

}
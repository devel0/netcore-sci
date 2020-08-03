using QuantumConcepts.Formats.StereoLithography;

namespace SearchAThing
{

    public static partial class SciExt
    {

        /// <summary>
        /// retrieve mean point of facet vertices
        /// </summary>
        /// <param name="facet">stl facet</param>
        /// <returns>mean point of face vertices</returns>
        public static Vector3D Mean(this Facet facet)
        {
            var x = 0d;
            var y = 0d;
            var z = 0d;

            int n = 0;

            foreach (var v in facet.Vertices)
            {
                x += v.X;
                y += v.Y;
                z += v.Z;

                ++n;
            }

            return new Vector3D(x, y, z) / n;
        }

    }

}
using SearchAThing.Util;

namespace SearchAThing.Sci
{

    public class Plane3D
    {

        public CoordinateSystem3D CS { get; private set; }

        /// <summary>
        /// XY(z) plane : top view
        /// </summary>
        public static readonly Plane3D XY = new Plane3D(CoordinateSystem3D.XY);

        /// <summary>
        /// XZ(-y) plane : front view
        /// </summary>
        public static readonly Plane3D XZ = new Plane3D(CoordinateSystem3D.XZ);

        /// <summary>
        /// YZ(x) plane : side view
        /// </summary>
        public static readonly Plane3D YZ = new Plane3D(CoordinateSystem3D.YZ);

        /// <summary>
        /// build plane using cs as reference for origin and x,y axes
        /// </summary>
        public Plane3D(CoordinateSystem3D cs)
        {
            CS = cs;
        }

        /// <summary>
        /// states if given wcs point is contained in this plane
        /// </summary>
        public bool Contains(double tol, Vector3D pt)
        {
            return pt.ToUCS(CS).Z.EqualsTol(tol, 0);
        }

        /// <summary>
        /// states if given wcs segment is contained in this plane
        /// </summary>
        public bool Contains(double tol, Line3D line)
        {
            return Contains(tol, line.From) && Contains(tol, line.To);
        }

    }


}

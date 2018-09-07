using SearchAThing.Util;

namespace SearchAThing.Sci
{

    public class Plane3D
    {

        public CoordinateSystem3D CS { get; private set; }
        
        /// <summary>
        /// XY(z) plane : top view
        /// </summary>
        public static Plane3D XY = new Plane3D(CoordinateSystem3D.XY);

        /// <summary>
        /// XZ(-y) plane : front view
        /// </summary>
        public static Plane3D XZ = new Plane3D(CoordinateSystem3D.XZ);        

        /// <summary>
        /// YZ(x) plane : side view
        /// </summary>
        public static Plane3D YZ = new Plane3D(CoordinateSystem3D.YZ);

        public Plane3D(CoordinateSystem3D cs)
        {
            CS = cs;            
        }

        public bool Contains(double tol, Vector3D pt)
        {
            return pt.ToUCS(CS).Z.EqualsTol(tol, 0);
        }

        public bool Contains(double tol, Line3D line)
        {
            return Contains(tol, line.From) && Contains(tol, line.To);
        }   

    }


}

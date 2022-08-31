using Newtonsoft.Json;

namespace SearchAThing
{

    /// <summary>
    /// encapsulate a CS to define a plane3d
    /// </summary>
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

#pragma warning disable CS8618
        [JsonConstructor]
        Plane3D()
        {
        }
#pragma warning restore

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
        public bool Contains(double tol, Vector3D pt) => pt.ToUCS(CS).Z.EqualsTol(tol, 0);

        /// <summary>
        /// states if given wcs segment is contained in this plane
        /// </summary>
        public bool Contains(double tol, Line3D line) => Contains(tol, line.From) && Contains(tol, line.To);

        /// <summary>
        /// states if given edge contained in this plane
        /// </summary>        
        public bool Contains(double tol, Edge edge) => Contains(tol, edge.SGeomFrom) && Contains(tol, edge.MidPoint) && Contains(tol, edge.SGeomTo);

        /// <summary>
        /// return intersection line between two planes or null if they parallels
        /// </summary>
        /// <param name="tol">len tolerance</param>
        /// <param name="other">other plane</param>        
        public Line3D? Intersect(double tol, Plane3D other) => CS.Intersect(tol, other.CS);

        public Plane3D Move(Vector3D delta) => new Plane3D(CS.Move(delta));

        /// <summary>
        /// retrieve the plane that is in the middle between this and other given
        /// </summary>
        public Plane3D MiddlePlane(double tol, Plane3D other)
        {
            if (this.CS.IsParallelTo(tol, other.CS))
            {
                var moveDst = other.CS.Origin.ToUCS(this.CS).Z * this.CS.BaseZ / 2;
                return this.Move(moveDst);
            }
            else
            {
                var iline = this.CS.Intersect(tol, other.CS);
                if (iline is null) throw new System.Exception($"can't find intersect line between planes {this} and {other}");
                var mline = iline.From.LineV(this.CS.BaseZ + other.CS.BaseZ);

                var mcs = new CoordinateSystem3D(iline.From, iline.V, mline.V, SmartCsMode.X_YQ);

                return new Plane3D(mcs);
            }
        }

        public string ToString(int digits) => CS.ToString(digits);

        public override string ToString() => ToString(3);

    }

}

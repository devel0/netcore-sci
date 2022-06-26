using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;

using netDxf.Entities;

namespace SearchAThing
{

    /// <summary>
    /// Specialized version of Arc3D with StartAngle=0 and EndAngle=2PI
    /// </summary>
    public class Circle3D : Arc3D
    {        

        /// <summary>
        /// create a circle in the given cs with given radius
        /// </summary>
        /// <param name="tol">length tolerance</param>
        /// <param name="cs"></param>
        /// <param name="r"></param>        
        public Circle3D(CoordinateSystem3D cs, double r) : base(cs, r, 0, 2 * PI)
        {
            GeomType = GeometryType.Circle3D;
        }

        /// <summary>
        /// create circle from given arc
        /// </summary>
        /// <param name="arc">arc used to build circle</param>
        public Circle3D(Arc3D arc) : base(arc.From, arc.MidPoint, arc.To)
        {
            GeomType = GeometryType.Circle3D;
        }

        /// <summary>
        /// Build 3d circle that intersect p1,p2,p3
        /// ( the inside CS will centered in the circle center and Xaxis toward p1 )
        /// </summary>        
        public Circle3D(Vector3D p1, Vector3D p2, Vector3D p3) : base(p1, p2, p3)
        {
            GeomType = GeometryType.Circle3D;
        }

        /// <summary>
        /// create a circle copy with origin moved
        /// </summary>
        /// <param name="delta">new circle origin delta</param>        
        public Circle3D Move(Vector3D delta) => new Circle3D(CS.Move(delta), Radius);

        /// <summary>
        /// build dxf circle
        /// </summary>        
        public override EntityObject DxfEntity
        {
            get
            {
                var c = new netDxf.Entities.Circle(CS.Origin, Radius);
                c.Normal = CS.BaseZ;
                return c;
            }
        }

        public override bool Contains(double tol, Vector3D pt, bool onlyPerimeter) =>
            Contains(tol, pt, inArcAngleRange: false, onlyPerimeter: onlyPerimeter);

        /// <summary>
        /// creates circle inscribed polygon and retrieve vertexes ( last == first )
        /// </summary>
        /// <param name="tol">length tolerance</param>
        /// <param name="segmentCount">count of inscribed polygon segments ( must at least 3; default is 360 )</param>
        /// <returns>coordinates of polygon vertices ( last == first )</returns>
        public IEnumerable<Vector3D> InscribedPolygon(double tol, int segmentCount = 360)
        {
            if (segmentCount < 3) throw new Exception($"segmentCount must >= 3");

            var alpha_step = 2 * PI / segmentCount;
            var alpha = 0.0;
            var alpha_stop = 2 * PI;

            var origPt = new Vector3D(Radius, 0);
            Vector3D prevPt = origPt;
            Vector3D firstPt = origPt.ToWCS(CS);

            yield return firstPt;

            alpha += alpha_step;

            Vector3D? nextPt = null;

            while (alpha < alpha_stop)
            {
                nextPt = origPt.RotateAboutZAxis(alpha);

                yield return nextPt.ToWCS(CS);

                prevPt = nextPt;

                alpha += alpha_step;
            }

            if (nextPt == null) yield break;

            if (!nextPt.EqualsTol(tol, origPt)) yield return firstPt;
        }

        /// <summary>
        /// build 3d circle that tangent to lines t1,t2 and that intersects point p
        /// note: point p must contained in one of t1,t2
        /// circle will be inside region t1.V toward t2.V
        /// they are 4 circles
        /// </summary>
        /// <param name="tol">length tolerance</param>
        /// <param name="t1">first tangent line</param>
        /// <param name="t2">second tangent line</param>
        /// <param name="p">point on t1 xor t2 to state the circle radius</param>        
        public static IEnumerable<Circle3D> CirclesTan12P(double tol, Line3D t1, Line3D t2, Vector3D p)
        {
            foreach (var da in new double[] { 0, PI / 2 })
            {
                var ip = t1.Intersect(tol, t2);
                if (ip == null) throw new Exception($"null intersect");

                var angle = t1.V.AngleRad(tol, t2.V);
                var t3 = new Line3D(ip, t1.V.RotateAs(tol, t1.V, t2.V, .5, da), Line3DConstructMode.PointAndVector);

                Line3D? lp = null;
                Line3D? lNp = null;
                if (t1.LineContainsPoint(tol, p)) { lp = t1; lNp = t2; }
                else if (t2.LineContainsPoint(tol, p)) { lp = t2; lNp = t1; }
                else throw new Exception($"circle 2 tan 1 point : pt must contained in one of given tan");

                var lpp = new Line3D(p, lp.V.RotateAboutAxis(t1.V.CrossProduct(t2.V), PI / 2), Line3DConstructMode.PointAndVector);
                var c = lpp.Intersect(tol, t3);

                var Radius = p.Distance(c);
                var CS = new CoordinateSystem3D(c, lpp.V, t2.V);

                yield return new Circle3D(CS, Radius);

                // mirrored addictional circle

                var mc = c.Mirror(new Line3D(p, p.Project(lNp) - p, Line3DConstructMode.PointAndVector));

                yield return new Circle3D(new CoordinateSystem3D(mc, lpp.V, t2.V), Radius);
            }
        }

        /// <summary>
        /// build 3d circle through point p, tangent to given t line, with given radius r            
        /// they can be two
        /// </summary>
        /// <param name="tol">length tolerance</param>
        /// <param name="p">passing point</param>
        /// <param name="t">tangent line</param>
        /// <param name="r">circle radius</param>        
        public static IEnumerable<Circle3D> CircleRTanP(double tol, Vector3D p, Line3D t, double r)
        {
            var pp = p.Project(t);
            var alpha = Asin((r - pp.Distance(p)) / r);
            var beta = PI / 2 - alpha;

            var axisz = (p - pp).CrossProduct(t.V);

            var t2 = new Line3D(p, t.V.RotateAboutAxis(axisz, beta), Line3DConstructMode.PointAndVector);

            return CirclesTan12P(tol, t, t2, p).Where(w => w.Radius.EqualsTol(tol, r));
        }

        /// <summary>
        /// intersect this 3d circle with given 3d line
        /// </summary>            
        public override IEnumerable<Vector3D> Intersect(double tol, Line3D l,
            bool only_perimeter = true,
            bool segment_mode = false) => base.Intersect(tol, l, only_perimeter, segment_mode, circle_mode: true);

        /// <summary>
        /// intersect this circle with given other;
        /// pre-requisite: circles are not the same one
        /// ; actually implemented only for coplanar circles
        /// </summary>
        /// <param name="tol">len tolerance</param>
        /// <param name="other">other circle</param>
        /// [unit test](https://github.com/devel0/netcore-sci/tree/master/test/Circle3D/Circle3DTest_0001.cs)
        /// <returns></returns>
        public IEnumerable<Vector3D> Intersect(double tol, Circle3D other)
        {
            if (this.EqualsTol(tol, other))
                throw new ArgumentException("circles are same");

            if (this.CS.IsParallelTo(tol, other.CS))
            {
                var centersDst = Center.Distance(other.Center);
                var rsum = Radius + other.Radius;

                // check if circles couldn't intersect
                if (centersDst.GreatThanTol(tol, rsum))
                    yield break;

                if (centersDst.EqualsTol(tol, rsum))
                {
                    var q = Intersect(tol, Center.LineTo(other.Center)).First();
                    yield return q;
                    yield break;
                }

                Vector3D P1, P2;
                {
                    // https://math.stackexchange.com/a/1367732/688020

                    var _center = Center.ToUCS(CS);
                    var _otherCenter = other.Center.ToUCS(CS);

                    var r1 = Radius;
                    var r2 = other.Radius;
                    var x1 = _center.X;
                    var y1 = _center.Y;
                    var x2 = _otherCenter.X;
                    var y2 = _otherCenter.Y;
                    var dx = x2 - x1;
                    var dy = y2 - y1;
                    var R = Sqrt(dx * dx + dy * dy);
                    var R2 = R * R;
                    var R4 = R2 * R2;
                    var r1pow2 = r1 * r1;
                    var r2pow2 = r2 * r2;
                    var r12pow2diff = r1pow2 - r2pow2;
                    var r12pow2sum = r1pow2 + r2pow2;
                    var a = r12pow2diff / (2 * R2);
                    var c = Sqrt(2 * r12pow2sum / R2 - (r12pow2diff * r12pow2diff) / R4 - 1);
                    var fx = (x1 + x2) / 2 + a * (x2 - x1);
                    var gx = c * (y2 - y1) / 2;
                    var fy = (y1 + y2) / 2 + a * (y2 - y1);
                    var gy = c * (x1 - x2) / 2;

                    P1 = new Vector3D(fx + gx, fy + gy).ToWCS(CS);
                    P2 = new Vector3D(fx - gx, fy - gy).ToWCS(CS);
                }

                yield return P1;
                yield return P2;
            }
            else
            {
                throw new NotImplementedException("not coplanar 3d circles not implemented yet");
            }
        }

        /// <summary>
        /// Circle area
        /// </summary>
        public double Area => PI * Radius * Radius;

        /// <summary>
        /// Circle perimeter
        /// </summary>        
        public override double Length => 2 * PI * Radius;

    }

    public static partial class SciExt
    {

        /// <summary>
        /// build circle3d by given set of 3 points
        /// </summary>        
        public static Circle3D CircleBy3Points(this IEnumerable<Vector3D> _pts)
        {
            var pts = _pts.ToArray();
            if (pts.Length != 3) throw new Exception("expected 3 points for circle3d");

            return new Circle3D(pts[0], pts[1], pts[2]);
        }

        /// <summary>
        /// build circle3d by provided dxf circle
        /// </summary>        
        public static Circle3D ToCircle3D(this netDxf.Entities.Circle dxf_circle) =>
            new Circle3D(new CoordinateSystem3D(dxf_circle.Center, dxf_circle.Normal, CoordinateSystem3DAutoEnum.AAA),
                dxf_circle.Radius);

    }

}
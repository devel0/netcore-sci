using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using netDxf.Entities;

namespace SearchAThing
{

    public class Circle3D : Arc3D
    {

        public Circle3D(double tol_len, CoordinateSystem3D cs, double r) : base(tol_len, cs, r, 0, 2 * PI)
        {
            Type = GeometryType.Circle3D;
        }

        /// <summary>
        /// create circle from given arc
        /// </summary>
        /// <param name="arc">arc used to build circle</param>
        public Circle3D(Arc3D arc) : base(arc.From, arc.MidPoint, arc.To)
        {
            Type = GeometryType.Circle3D;
        }

        /// <summary>
        /// Build 3d circle that intersect p1,p2,p3
        /// ( the inside CS will centered in the circle center and Xaxis toward p1 )
        /// </summary>        
        public Circle3D(Vector3D p1, Vector3D p2, Vector3D p3) : base(p1, p2, p3)
        {
            Type = GeometryType.Circle3D;
        }

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
        /// <param name="tolLen">length tolerance</param>
        /// <param name="segmentCount">count of inscribed polygon segments ( must at least 3; default is 360 )</param>
        /// <returns>coordinates of polygon vertices ( last == first )</returns>
        public IEnumerable<Vector3D> InscribedPolygon(double tolLen, int segmentCount = 360)
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

            Vector3D nextPt = null;

            while (alpha < alpha_stop)
            {
                nextPt = origPt.RotateAboutZAxis(alpha);

                yield return nextPt.ToWCS(CS);

                prevPt = nextPt;

                alpha += alpha_step;
            }

            if (nextPt == null) yield break;

            if (!nextPt.EqualsTol(tolLen, origPt)) yield return firstPt;
        }

        /// <summary>
        /// build 3d circle that tangent to lines t1,t2 and that intersects point p
        /// note: point p must contained in one of t1,t2
        /// circle will be inside region t1.V toward t2.V
        /// they are 4 circles
        /// </summary>            
        public static IEnumerable<Circle3D> CirclesTan12P(double tol_len, Line3D t1, Line3D t2, Vector3D p)
        {
            foreach (var da in new double[] { 0, PI / 2 })
            {
                var ip = t1.Intersect(tol_len, t2);
                var angle = t1.V.AngleRad(tol_len, t2.V);
                var t3 = new Line3D(ip, t1.V.RotateAs(tol_len, t1.V, t2.V, .5, da), Line3DConstructMode.PointAndVector);

                Line3D lp = null;
                Line3D lNp = null;
                if (t1.LineContainsPoint(tol_len, p)) { lp = t1; lNp = t2; }
                else if (t2.LineContainsPoint(tol_len, p)) { lp = t2; lNp = t1; }
                else throw new Exception($"circle 2 tan 1 point : pt must contained in one of given tan");

                var lpp = new Line3D(p, lp.V.RotateAboutAxis(t1.V.CrossProduct(t2.V), PI / 2), Line3DConstructMode.PointAndVector);
                var c = lpp.Intersect(tol_len, t3);

                var Radius = p.Distance(c);
                var CS = new CoordinateSystem3D(c, lpp.V, t2.V);

                yield return new Circle3D(tol_len, CS, Radius);

                // mirrored addictional circle

                var mc = c.Mirror(new Line3D(p, p.Project(lNp) - p, Line3DConstructMode.PointAndVector));

                yield return new Circle3D(tol_len, new CoordinateSystem3D(mc, lpp.V, t2.V), Radius);
            }
        }

        /// <summary>
        /// build 3d circle through point p, tangent to given t line, with given radius r            
        /// they can be two
        /// </summary>            
        public static IEnumerable<Circle3D> CircleRTanP(double tol_len, Vector3D p, Line3D t, double r)
        {
            var pp = p.Project(t);
            var alpha = Asin((r - pp.Distance(p)) / r);
            var beta = PI / 2 - alpha;

            var axisz = (p - pp).CrossProduct(t.V);

            var t2 = new Line3D(p, t.V.RotateAboutAxis(axisz, beta), Line3DConstructMode.PointAndVector);

            return CirclesTan12P(tol_len, t, t2, p).Where(w => w.Radius.EqualsTol(tol_len, r));
        }

        /// <summary>
        /// intersect this 3d circle with given 3d line
        /// </summary>            
        public override IEnumerable<Vector3D> Intersect(double tol, Line3D l,
            bool only_perimeter = true,
            bool segment_mode = false)
        {
            return base.Intersect(tol, l, only_perimeter, segment_mode, circle_mode: true);
        }

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

                // http://www.ambrsoft.com/TrigoCalc/Circles2/circle2intersection/CircleCircleIntersection.htm

                var _center = Center.ToUCS(CS);
                var _otherCenter = other.Center.ToUCS(CS);

                var c1_a = _center.X;
                var c1_b = _center.Y;
                var c2_a = _otherCenter.X;
                var c2_b = _otherCenter.Y;
                var c1_r = Radius;
                var c2_r = other.Radius;
                var D = centersDst;

                var a1 = D + c1_r + c2_r;
                var a2 = D + c1_r - c2_r;
                var a3 = D - c1_r + c2_r;
                var a4 = -D + c1_r + c2_r;

                var area = Sqrt(a1 * a2 * a3 * a4) / 4;

                var val1 = (c1_a + c2_a) / 2 + (c2_a - c1_a) * (c1_r * c1_r - c2_r * c2_r) / (2 * D * D);
                var val2 = 2 * (c1_b - c2_b) * area / (D * D);

                var x1 = val1 + val2;
                var x2 = val1 - val2;

                val1 = (c1_b + c2_b) / 2 + (c2_b - c1_b) * (c1_r * c1_r - c2_r * c2_r) / (2 * D * D);
                val2 = 2 * (c1_a - c2_a) * area / (D * D);

                var y1 = val1 - val2;
                var y2 = val1 + val2;

                var test = Abs((x1 - c1_a) * (x1 - c1_a) + (y1 - c1_b) * (y1 - c1_b) - c1_r * c1_r);
                if (test > tol)
                {
                    var tmp = y1;
                    y1 = y2;
                    y2 = tmp;
                }

                var P1 = new Vector3D(x1, y1).ToWCS(CS);
                var P2 = new Vector3D(x2, y2).ToWCS(CS);

                yield return P1;
                yield return P2;
            }
            else
            {
                throw new NotImplementedException("not coplanar 3d circles not implemented yet");
            }
        }

        public double Area { get { return PI * Radius * Radius; } }

        /// <summary>
        /// Circle perimeter
        /// </summary>        
        public override double Length { get { return 2 * PI * Radius; } }

    }

    public static partial class SciExt
    {

        public static Circle3D CircleBy3Points(this IEnumerable<Vector3D> _pts)
        {
            var pts = _pts.ToArray();
            if (pts.Length != 3) throw new Exception("expected 3 points for circle3d");

            return new Circle3D(pts[0], pts[1], pts[2]);
        }

        public static Circle3D ToCircle3D(this netDxf.Entities.Circle dxf_circle, double tol_len)
        {
            return new Circle3D(tol_len, new CoordinateSystem3D(dxf_circle.Center, dxf_circle.Normal, CoordinateSystem3DAutoEnum.AAA), dxf_circle.Radius);
        }

    }

}
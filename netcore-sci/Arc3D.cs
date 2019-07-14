using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using SearchAThing.Sci;
using netDxf.Entities;
using SearchAThing.Util;
using System.Diagnostics.CodeAnalysis;

namespace SearchAThing
{

    namespace Sci
    {

        /// <summary>
        /// base geometry for arc 3d entities
        /// </summary>
        public class Arc3D : Geometry
        {

            /// <summary>
            /// coordinate system centered in arc center
            /// angle is 0 at X axis
            /// angle increase rotating right-hand on Z axis
            /// </summary>            
            public CoordinateSystem3D CS { get; private set; }

            /// <summary>
            /// radius of arc
            /// </summary>            
            public double Radius { get; private set; }

            private double tol_rad;

            /// <summary>
            /// construct 3d arc
            /// </summary>
            /// <param name="cs">coordinate system with origin at arc center, XY plane of cs contains the arc, angle is 0 at cs x-axis and increase right-hand around cs z-axis</param>
            /// <param name="r">arc radius</param>
            /// <param name="angleRadStart">arc angle start (rad). is not required that start angle less than end. It will normalized 0-2pi</param>
            /// <param name="angleRadEnd">arc angle end (rad). is not require that end angle great than start. It will normalized 0-2pi</param>
            /// <returns>3d arc</returns>
            public Arc3D(double tol_len, CoordinateSystem3D cs, double r, double angleRadStart, double angleRadEnd) :
                base(GeometryType.Arc3D)
            {
                tol_rad = tol_len.RadTol(r);
                AngleStart = angleRadStart.NormalizeAngle2PI(tol_rad);
                AngleEnd = angleRadEnd.NormalizeAngle2PI(tol_rad);
                CS = cs;
                Radius = r;
            }

            /// <summary>
            /// helper to build circle by given 3 points
            /// </summary>
            /// <param name="p1">first constraint point</param>            
            /// <param name="p2">second constraint point</param>            
            /// <param name="p3">third constraint point</param>            
            /// <returns>cs and radius that describe a 3d circle</returns>
            public static (CoordinateSystem3D CS, double Radius) CircleBy3Points(Vector3D p1, Vector3D p2, Vector3D p3)
            {
                // https://en.wikipedia.org/wiki/Circumscribed_circle
                // Cartesian coordinates from cross- and dot-products

                var d = ((p1 - p2).CrossProduct(p2 - p3)).Length;

                var Radius = ((p1 - p2).Length * (p2 - p3).Length * (p3 - p1).Length) / (2 * d);

                var alpha = Pow((p2 - p3).Length, 2) * (p1 - p2).DotProduct(p1 - p3) / (2 * Pow(d, 2));
                var beta = Pow((p1 - p3).Length, 2) * (p2 - p1).DotProduct(p2 - p3) / (2 * Pow(d, 2));
                var gamma = Pow((p1 - p2).Length, 2) * (p3 - p1).DotProduct(p3 - p2) / (2 * Pow(d, 2));

                var c = alpha * p1 + beta * p2 + gamma * p3;

                var CS = new CoordinateSystem3D(c, p1 - c, p2 - c);

                return (CS, Radius);
            }

            /// <summary>
            /// build 3d arc by given 3 points
            /// </summary>
            /// <param name="p1">first constraint point</param>
            /// <param name="p2">second constraint point</param>
            /// <param name="p3">third constraint point</param>
            /// <returns>3d arc passing for given points with angles 0-2pi</returns>
            internal Arc3D(Vector3D p1, Vector3D p2, Vector3D p3) :
                base(GeometryType.Arc3D)
            {
                Type = GeometryType.Arc3D;

                var nfo = Arc3D.CircleBy3Points(p1, p2, p3);

                CS = nfo.CS;
                Radius = nfo.Radius;

                AngleStart = 0;
                AngleEnd = 2 * PI;
            }

            /// <summary>
            /// Build arc by 3 given points
            /// ( the inside CS will centered in the arc center and Xaxis toward p1 )
            /// </summary>        
            public Arc3D(double tol_len, Vector3D p1, Vector3D p2, Vector3D p3, Vector3D normal = null) :
                base(GeometryType.Arc3D)
            {
                Type = GeometryType.Arc3D;

                var nfo = Arc3D.CircleBy3Points(p1, p2, p3);

                CS = nfo.CS;
                Radius = nfo.Radius;

                if (normal != null)
                {
                    if (!normal.Colinear(tol_len, CS.BaseZ)) throw new ArgumentException($"invalid given normal not colinear to arc axis");
                    if (!normal.Concordant(tol_len, CS.BaseZ))
                    {
                        CS = CS.Rotate(CS.BaseX, PI);
                    }
                }

                tol_rad = tol_len.RadTol(Radius);
                AngleStart = CS.BaseX.AngleToward(tol_len, p1 - CS.Origin, CS.BaseZ).NormalizeAngle2PI(tol_rad);
                AngleEnd = CS.BaseX.AngleToward(tol_len, p3 - CS.Origin, CS.BaseZ).NormalizeAngle2PI(tol_rad);
            }

            /// <summary>
            /// create an arc copy with origin moved
            /// </summary>
            /// <param name="delta">new arc origin delta</param>
            public Arc3D Move(double tol_len, Vector3D delta)
            {
                return new Arc3D(tol_len, CS.Move(delta), Radius, AngleStart, AngleEnd);
            }

            /// <summary>
            /// start angle (rad) [0-2pi) respect cs xaxis rotating around cs zaxis
            /// note that start angle can be greather than end angle
            /// </summary>
            public double AngleStart { get; private set; }

            /// <summary>
            /// end angle (rad) [0-2pi) respect cs xaxis rotating around cs zaxis
            /// note that start angle can be greather than end angle
            /// </summary>            
            public double AngleEnd { get; private set; }

            /// <summary>
            /// Arc (rad) angle length.
            /// angle between start-end or end-start depending on what start is less than end or not
            /// </summary>
            public double Angle
            {
                get
                {
                    return AngleStart.Angle(tol_rad, AngleEnd);
                }
            }

            /// <summary>
            /// Length of Arc from start to end
            /// </summary>
            public override double Length
            {
                get
                {
                    return Angle * Radius;
                }
            }

            public override Vector3D GeomFrom => From;

            public override Vector3D GeomTo => To;

            /// <summary>
            /// point on the arc circumnfere at given angle (rotating cs basex around cs basez)
            /// note: it start
            /// </summary>
            public Vector3D PtAtAngle(double angleRad)
            {
                return (Vector3D.XAxis * Radius).RotateAboutZAxis(angleRad).ToWCS(CS);
            }

            /// <summary>
            /// mid point eval as arc point at angle start + arc angle/2
            /// </summary>
            public Vector3D MidPoint
            {
                get
                {
                    return PtAtAngle(AngleStart + Angle / 2);
                }
            }

            /// <summary>            
            /// return the angle (rad) of the point respect cs x axis rotating around cs z axis
            /// to reach given point angle alignment
            /// </summary>            
            public double PtAngle(double tolLen, Vector3D pt)
            {
                var v_x = CS.BaseX;
                var v_pt = pt - CS.Origin;

                return v_x.AngleToward(tolLen, v_pt, CS.BaseZ);
            }

            /// <summary>
            /// point at angle start
            /// </summary>
            public Vector3D From { get { return PtAtAngle(AngleStart); } }

            /// <summary>
            /// point at angle end
            /// </summary>            
            public Vector3D To { get { return PtAtAngle(AngleEnd); } }

            /// <summary>
            /// return From,To segment
            /// </summary>
            public Line3D Segment { get { return new Line3D(From, To); } }

            public override IEnumerable<Vector3D> Vertexes
            {
                get
                {
                    yield return From;
                    yield return To;
                }
            }

            /// <summary>
            /// Checks if two arcs are equals ( it checks agains swapped from-to too )
            /// </summary>        
            public bool EqualsTol(double tolLen, Arc3D other)
            {
                if (!Center.EqualsTol(tolLen, other.Center)) return false;
                if (!Radius.EqualsTol(tolLen, other.Radius)) return false;
                if (!Segment.EqualsTol(tolLen, other.Segment)) return false;
                return true;
            }

            /// <summary>
            /// http://www.lee-mac.com/bulgeconversion.html
            /// </summary>            
            public double Bulge(double tolLen, Vector3D from, Vector3D to, Vector3D N)
            {
                var v1 = from - Center;
                var v2 = to - Center;
                var ang = v1.AngleToward(tolLen, v2, CS.BaseZ);
                var factor = 1.0;
                if (!N.Concordant(Constants.NormalizedLengthTolerance, CS.BaseZ))
                    factor = -1.0;

                return Tan(ang / 4) * factor;
            }

            /// <summary>
            /// statis if given point contained in arc perimeter/shape or circle perimeter/shape depending on specified mode
            /// </summary>                
            /// <param name="tol_len">len tolerance</param>
            /// <param name="pt">point to test</param>
            /// <param name="inArcAngleRange">true if point angle must contained in arc angles, false to test like a circle</param>
            /// <param name="onlyPerimeter">true to test point contained only in perimeter, false to test also contained in area</param>
            /// <returns></returns>//         
            protected bool Contains(double tol_len, Vector3D pt,
                bool inArcAngleRange, bool onlyPerimeter)
            {
                var onplane = pt.ToUCS(CS).Z.EqualsTol(tol_len, 0);
                var center_dst = pt.Distance(CS.Origin);

                if (inArcAngleRange)
                {
                    var tol_rad = tol_len.RadTol(Radius);
                    var ptAngle = PtAngle(tol_len, pt);
                    var isInAngleRange = ptAngle.AngleInRange(tol_rad, AngleStart, AngleEnd);
                    if (!isInAngleRange) return false;
                }

                if (onlyPerimeter)
                    return onplane && center_dst.EqualsTol(tol_len, Radius);
                else
                    return onplane && center_dst.LessThanOrEqualsTol(tol_len, Radius);
            }

            /// <summary>
            /// states if given point relies on this arc perimeter or shape depending on arguments
            /// </summary>
            /// <param name="tol">length tolerance</param>
            /// <param name="pt">point to check</param>
            /// <param name="onlyPerimeter">if true it checks if point is on perimeter; if false it will check in area too</param>
            public virtual bool Contains(double tol, Vector3D pt, bool onlyPerimeter) =>
                Contains(tol, pt, true, onlyPerimeter);

            public Vector3D Center { get { return CS.Origin; } }

            /// <summary>
            /// centre of mass of circular segment
            /// </summary>            
            public Vector3D CentreOfMass(out double A)
            {
                // https://en.wikipedia.org/wiki/List_of_centroids

                var alpha = Angle / 2;
                A = Pow(Radius, 2) / 2 * (2 * alpha - Sin(2 * alpha));

                var x = (4 * Radius * Pow(Sin(alpha), 3)) / (3 * (2 * alpha - Sin(2 * alpha)));

                return Center + (MidPoint - Center).Normalized() * x;
            }

            public override EntityObject DxfEntity
            {
                get
                {
                    var dxf_cs = new CoordinateSystem3D(CS.Origin, CS.BaseZ, CoordinateSystem3DAutoEnum.AAA);
                    var astart = dxf_cs.BaseX.AngleToward(Constants.NormalizedLengthTolerance, From - CS.Origin, CS.BaseZ);
                    var aend = dxf_cs.BaseX.AngleToward(Constants.NormalizedLengthTolerance, To - CS.Origin, CS.BaseZ);
                    var arc = new Arc(Center, Radius, astart.ToDeg(), aend.ToDeg());
                    arc.Normal = CS.BaseZ;
                    return arc;
                }
            }

            private double SearchOrd(double tol_len, int ord, double angleFrom, double angleTo, bool ltOrGt)
            {
                var ang = angleFrom.Angle(tol_rad, angleTo);

                var fromVal = PtAtAngle(angleFrom).GetOrd(ord);
                var midVal = PtAtAngle(angleFrom + ang / 2).GetOrd(ord);
                var toVal = PtAtAngle(angleTo).GetOrd(ord);

                if (ltOrGt)
                {
                    if (fromVal.LessThanTol(tol_len, toVal))
                    {
                        if (fromVal.EqualsTol(tol_len, midVal)) return (fromVal + midVal) / 2;
                        return SearchOrd(tol_len, ord, angleFrom, angleFrom + ang / 2, ltOrGt);
                    }
                    else // to < from
                    {
                        if (midVal.EqualsTol(tol_len, toVal)) return (midVal + toVal) / 2;
                        return SearchOrd(tol_len, ord, angleFrom + ang / 2, angleTo, ltOrGt);
                    }
                }
                else
                {
                    if (fromVal.GreatThanTol(tol_len, toVal))
                    {
                        if (fromVal.EqualsTol(tol_len, midVal)) return fromVal;
                        return SearchOrd(tol_len, ord, angleFrom, angleFrom + ang / 2, ltOrGt);
                    }
                    else // to < from
                    {
                        if (midVal.EqualsTol(tol_len, toVal)) return toVal;
                        return SearchOrd(tol_len, ord, angleFrom + ang / 2, angleTo, ltOrGt);
                    }
                }
            }

            /// <summary>
            /// compute wcs bbox executing a recursive bisect search of min and max
            /// </summary>
            public override BBox3D BBox(double tol_len)
            {
                return new BBox3D(new[] {
                    new Vector3D(
                        SearchOrd(tol_len, 0, AngleStart, AngleEnd, ltOrGt: true), // xmin
                        SearchOrd(tol_len, 1, AngleStart, AngleEnd, ltOrGt: true), // ymin
                        SearchOrd(tol_len, 2, AngleStart, AngleEnd, ltOrGt: true)), // zmin
                    new Vector3D(
                        SearchOrd(tol_len, 0, AngleStart, AngleEnd, ltOrGt: false), // xmax
                        SearchOrd(tol_len, 1, AngleStart, AngleEnd, ltOrGt: false), // ymax
                        SearchOrd(tol_len, 2, AngleStart, AngleEnd, ltOrGt: false)) // zmax
                });
            }

            /// <summary>
            /// intersect this 3d circle with given 3d line
            /// </summary>            
            private IEnumerable<Vector3D> IntersectCircle(double tol, Line3D l, bool segment_mode = false)
            {
                var lprj = new Line3D(l.From.ToUCS(CS).Set(OrdIdx.Z, 0), l.To.ToUCS(CS).Set(OrdIdx.Z, 0));

                var a = Pow(lprj.To.X - lprj.From.X, 2) + Pow(lprj.To.Y - lprj.From.Y, 2);
                var b = 2 * lprj.From.X * (lprj.To.X - lprj.From.X) + 2 * lprj.From.Y * (lprj.To.Y - lprj.From.Y);
                var c = Pow(lprj.From.X, 2) + Pow(lprj.From.Y, 2) - Pow(Radius, 2);
                var d = Pow(b, 2) - 4 * a * c;

                if (d.LessThanTol(tol, 0)) yield break; // no intersection at all

                var sd = Sqrt(Abs(d));
                var f1 = (-b + sd) / (2 * a);
                var f2 = (-b - sd) / (2 * a);

                // one intersection point is
                var ip = new Vector3D(
                    lprj.From.X + (lprj.To.X - lprj.From.X) * f1,
                    lprj.From.Y + (lprj.To.Y - lprj.From.Y) * f1,
                    0);

                Vector3D ip2 = null;

                if (!f1.EqualsTol(Constants.NormalizedLengthTolerance, f2))
                {
                    // second intersection point is
                    ip2 = new Vector3D(
                        lprj.From.X + (lprj.To.X - lprj.From.X) * f2,
                        lprj.From.Y + (lprj.To.Y - lprj.From.Y) * f2,
                        0);
                }

                // back to wcs, check line contains point
                var wcs_ip = ip.ToWCS(CS);
                Vector3D wcs_ip2 = null;
                if (ip2 != null) wcs_ip2 = ip2.ToWCS(CS);

                if (l.LineContainsPoint(tol, wcs_ip, segment_mode))
                    yield return wcs_ip;

                if (ip2 != null && l.LineContainsPoint(tol, wcs_ip2, segment_mode))
                    yield return wcs_ip2;
            }

            /// <summary>
            /// states if this arc intersect given line
            /// </summary>
            /// <param name="tol">arc tolerance</param>
            /// <param name="l">line to test intersect</param>
            /// <param name="segment_mode">if true line treat as segment instead of infinite</param>            
            /// <param name="circle_mode">if true arc treat as circle</param>            
            protected IEnumerable<Vector3D> Intersect(double tol, Line3D l,
                bool only_perimeter,
                bool segment_mode,
                bool circle_mode)
            {
                var cmp = new Vector3DEqualityComparer(tol);
                var res = new HashSet<Vector3D>(cmp);

                foreach (var x in IntersectCircle(tol, l, segment_mode))
                {
                    res.Add(x);
                }

                if (!only_perimeter)
                {
                    var c_f = new Line3D(Center, From);
                    {
                        var q_c_f = c_f.Intersect(tol, l);
                        if (q_c_f != null) res.Add(q_c_f);
                    }

                    if (!circle_mode)
                    {
                        var c_e = new Line3D(Center, To);
                        {
                            var q_c_e = c_e.Intersect(tol, l);
                            if (q_c_e != null) res.Add(q_c_e);
                        }
                    }
                }

                if (!circle_mode)
                    return res.Where(r => this.Contains(tol, r, onlyPerimeter: only_perimeter));

                return res;
            }

            /// <summary>
            /// find ips of intersection between this arc and given line
            /// </summary>
            /// <param name="tol">length tolerance</param>
            /// <param name="l">line</param>
            /// <param name="only_perimeter">check intersection only along perimeter; if false it will check intersection along arc area shape border too</param>
            /// <param name="segment_mode">if true treat given line as segment; if false as infinite line</param>
            /// <returns>intersection points between this arc and given line</returns>
            public virtual IEnumerable<Vector3D> Intersect(double tol, Line3D l,
                bool only_perimeter = true, bool segment_mode = false)
            {
                return Intersect(tol, l,
                    only_perimeter: only_perimeter,
                    segment_mode: segment_mode,
                    circle_mode: false);
            }

            /// <summary>
            /// find ips of intersect this arc to the given cs plane; 
            /// return empty set if arc cs plane parallel to other given cs
            /// </summary>            
            /// <param name="tol">len tolerance</param>
            /// <param name="cs">cs xy plane</param>
            /// <param name="only_perimeter">if false it will check in the arc area too, otherwise only on arc perimeter</param>
            /// <returns>sample</returns>
            /// <remarks>            
            /// [unit test](/test/Arc3D/Arc3DTest_0001.cs)
            /// ![](/test/Arc3D/Arc3DTest_0001.png)
            /// </remarks>            
            public IEnumerable<Vector3D> Intersect(double tol, CoordinateSystem3D cs,
                bool only_perimeter = true)
            {
                if (this.CS.IsParallelTo(tol, cs)) yield break;

                var iLine = this.CS.Intersect(tol, cs);

                foreach (var x in this.Intersect(tol, iLine,
                    only_perimeter: only_perimeter,
                    segment_mode: false,
                    circle_mode: false))
                    yield return x;
            }

            [ExcludeFromCodeCoverage]
            public override string ToString()
            {
                return $"C:{Center} r:{Round(Radius, 3)} ANGLE:{Round(Angle.ToDeg(), 1)}deg FROM[{From} {Round(AngleStart.ToDeg(), 1)} deg] TO[{To} {Round(AngleEnd.ToDeg(), 1)} deg]";
            }

            /// <summary>
            /// split arc into pieces and retrieve split points
            /// </summary>
            /// <param name="cnt">nr of piece</param>
            /// <param name="include_endpoints">if true returns also boundary points</param>
            public override IEnumerable<Vector3D> Divide(int cnt, bool include_endpoints = false)
            {
                var from = GeomFrom;
                if (include_endpoints) yield return from;

                var p = from;
                var ang_step = Angle / cnt;

                var ang = ang_step;
                var ax_rot = new Line3D(Center, CS.BaseZ, Line3DConstructMode.PointAndVector);

                for (int i = 0; i < cnt - 1; ++i)
                {
                    p = from.RotateAboutAxis(ax_rot, ang);
                    yield return p;

                    ang += ang_step;
                }

                if (include_endpoints) yield return GeomTo;
            }

            /// <summary>
            /// create a set of subarc from this by splitting through given split points
            /// split point are not required to be on perimeter of the arc ( a center arc to point line will split )
            /// generated subarcs will start from this arc angleFrom and contiguosly end to angleTo
            /// </summary>                        
            /// <param name="tol_len">arc length tolerance</param>
            /// <param name="_splitPts">point where split arc</param>
            /// <param name="validate_pts">if true split only for split points on arc perimeter</param>            
            public IEnumerable<Arc3D> Split(double tol_len, IEnumerable<Vector3D> _splitPts, bool validate_pts = false)
            {
                var tol_rad = tol_len.RadTol(Radius);

                if (_splitPts == null || _splitPts.Count() == 0) yield break;

                IEnumerable<Vector3D> splitPts = _splitPts;

                if (validate_pts) splitPts = _splitPts.Where(pt => Contains(tol_len, pt, onlyPerimeter: true)).ToList();

                var radCmp = new DoubleEqualityComparer(tol_rad);

                var hs_angles_rad = new HashSet<double>(radCmp);
                foreach (var splitPt in splitPts.Select(pt => PtAngle(tol_len, pt)))
                {
                    if (PtAtAngle(splitPt).EqualsTol(tol_len, From) || PtAtAngle(splitPt).EqualsTol(tol_len, To)) continue;
                    hs_angles_rad.Add(splitPt.NormalizeAngle2PI(tol_rad));
                }

                var angles_rad = hs_angles_rad.OrderBy(w => w).ToList();
                if (!hs_angles_rad.Contains(AngleStart)) angles_rad.Insert(0, AngleStart);
                if (!hs_angles_rad.Contains(AngleEnd)) angles_rad.Add(AngleEnd);

                for (int i = 0; i < angles_rad.Count - 1; ++i)
                {
                    var arc = new Arc3D(tol_len, CS, Radius, angles_rad[i], angles_rad[i + 1]);
                    yield return arc;
                }
            }

        }

    }

    public static partial class SciExt
    {

        /// <summary>
        /// compute angle rad tolerance by given arc length tolerance
        /// </summary>
        /// <param name="lenTol">length tolerance on the arc</param>
        /// <param name="radius">radius of the arc</param>
        public static double RadTol(this double lenTol, double radius) => lenTol / radius;

        public static Arc3D ToArc3D(this netDxf.Entities.Arc dxf_arc, double tolLen)
        {
            return new Arc3D(tolLen,
                new CoordinateSystem3D(dxf_arc.Center, dxf_arc.Normal, CoordinateSystem3DAutoEnum.AAA), dxf_arc.Radius,
                dxf_arc.StartAngle.ToRad(), dxf_arc.EndAngle.ToRad());
        }

        /// <summary>
        /// states if given angle is contained in from, to angle range;
        /// multiturn angles are supported because test will normalize to [0,2pi) automatically.
        /// </summary>                
        /// <param name="pt_angle">angle(rad) to test</param>
        /// <param name="tol_rad">angle(rad) tolerance</param>
        /// <param name="angle_from">angle(rad) from</param>
        /// <param name="angle_to">angle(rad) to</param>        
        public static bool AngleInRange(this double pt_angle, double tol_rad,
            double angle_from, double angle_to)
        {
            pt_angle = pt_angle.NormalizeAngle2PI(tol_rad);
            angle_from = angle_from.NormalizeAngle2PI(tol_rad);
            angle_to = angle_to.NormalizeAngle2PI(tol_rad);

            if (angle_from.GreatThanTol(tol_rad, angle_to))
            {
                return
                    pt_angle.LessThanOrEqualsTol(tol_rad, angle_to)
                    ||
                    pt_angle.GreatThanOrEqualsTol(tol_rad, angle_from);
            }
            else // from < to
            {
                return
                    pt_angle.GreatThanOrEqualsTol(tol_rad, angle_from)
                    &&
                    pt_angle.LessThanOrEqualsTol(tol_rad, angle_to);
            }
        }

    }

    /// <summary>
    /// checks if arcs share same plane, origin, radius, angle start-end
    /// </summary>
    public class Arc3DEqualityComparer : IEqualityComparer<Arc3D>
    {
        double tolLen;
        double tolRad;

        public Arc3DEqualityComparer(double _tolLen)
        {
            tolLen = _tolLen;
        }

        public bool Equals(Arc3D x, Arc3D y)
        {
            return x.EqualsTol(tolLen, y);
        }

        public int GetHashCode(Arc3D obj)
        {
            return 0;
        }

    }

}

using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using SearchAThing.Sci;
using netDxf.Entities;
using SearchAThing.Util;

namespace SearchAThing
{

    namespace Sci
    {

        public class Arc3D : Geometry
        {
            public CoordinateSystem3D CS { get; private set; }
            public double Radius { get; private set; }

            public Arc3D(CoordinateSystem3D cs, double r, double angleRadStart, double angleRadEnd) :
                base(GeometryType.Arc3D)
            {
                AngleStartRad = angleRadStart;
                AngleEndRad = angleRadEnd;
                CS = cs;
                Radius = r;
            }

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
            /// Build arc by 3 given points with angle 2*PI
            /// ( the inside CS will centered in the arc center and Xaxis toward p1 )
            /// </summary>        
            internal Arc3D(Vector3D p1, Vector3D p2, Vector3D p3) :
                base(GeometryType.Arc3D)
            {
                Type = GeometryType.Arc3D;

                var nfo = Arc3D.CircleBy3Points(p1, p2, p3);

                CS = nfo.CS;
                Radius = nfo.Radius;

                AngleStartRad = 0;
                AngleEndRad = 2 * PI;
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
                    if (!normal.Colinear(tol_len, CS.BaseZ)) throw new Exception($"invalid given normal not colinear to arc axis");
                    if (!normal.Concordant(tol_len, CS.BaseZ))
                    {
                        CS = CS.Rotate(CS.BaseX, PI);
                    }
                }

                AngleStartRad = CS.BaseX.AngleToward(tol_len, p1 - CS.Origin, CS.BaseZ);
                AngleEndRad = CS.BaseX.AngleToward(tol_len, p3 - CS.Origin, CS.BaseZ);
            }

            public Arc3D Move(Vector3D delta)
            {
                return new Arc3D(CS.Move(delta), Radius, AngleStartRad, AngleEndRad);
            }

            public double AngleStartRad { get; private set; }
            public double AngleEndRad { get; private set; }
            public double AngleRad
            {
                get
                {
                    var astart = AngleStartRad.NormalizeAngle2PI();
                    var aend = AngleEndRad.NormalizeAngle2PI();

                    if (astart > aend)
                        return aend + (2 * PI - astart);
                    else
                        return aend - astart;
                }
            }

            /// <summary>
            /// Length of Arc from start to end
            /// </summary>
            public override double Length
            {
                get
                {
                    return AngleRad * Radius;
                }
            }

            public override Vector3D GeomFrom => From;
            public override Vector3D GeomTo => To;

            public Vector3D PtAtAngle(double angleRad)
            {
                return (Vector3D.XAxis * Radius).RotateAboutZAxis(angleRad).ToWCS(CS);
            }

            public Vector3D MidPoint
            {
                get
                {
                    return PtAtAngle(AngleStartRad + AngleRad / 2);
                }
            }

            /// <summary>
            /// assuming pt is a point on the arc
            /// return the angle of the point ( rad )
            /// </summary>            
            public double PtAngle(double tolLen, Vector3D pt)
            {
                var v_x = CS.BaseX;
                var v_pt = pt - CS.Origin;

                return v_x.AngleToward(tolLen, v_pt, CS.BaseZ);
            }

            public Vector3D From { get { return PtAtAngle(AngleStartRad); } }
            public Vector3D To { get { return PtAtAngle(AngleEndRad); } }
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
            public bool EqualsTol(double tolLen, double tolRad, Arc3D other)
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
            /// check if this circle contains given point
            /// </summary>            
            public bool Contains(double tol, Vector3D pt, bool onlyAtCircumnfere = false)
            {
                var onplane = pt.ToUCS(CS).Z.EqualsTol(tol, 0);
                var center_dst = pt.Distance(CS.Origin);

                if (onlyAtCircumnfere)
                    return onplane && center_dst.EqualsTol(tol, Radius);
                else
                    return onplane && center_dst.LessThanOrEqualsTol(tol, Radius);
            }

            /// <summary>
            /// verify if given point is in this arc between its start-to arc angles
            /// </summary>            
            public bool Contains(double tolLen, double tolRad, Vector3D pt)
            {
                // if not in circle stop
                if (!this.Contains(tolLen, pt)) return false;

                return PtAngle(tolLen, pt).AngleInRange(tolRad, AngleStartRad, AngleEndRad);
            }

            /// <summary>
            /// if validate_pts false it assume all given split points are valid point on the arc
            /// </summary>            
            public IEnumerable<Arc3D> Split(double tolLen, double tolRad, IEnumerable<Vector3D> _splitPts, bool validate_pts = false)
            {
                if (_splitPts == null || _splitPts.Count() == 0) yield break;

                IEnumerable<Vector3D> splitPts = _splitPts;

                if (validate_pts) splitPts = _splitPts.Where(pt => Contains(tolLen, tolRad, pt)).ToList();

                var radCmp = new DoubleEqualityComparer(tolRad);

                var hs_angles_rad = new HashSet<double>(radCmp);
                foreach (var splitPt in splitPts.Select(pt => PtAngle(tolLen, pt)))
                {
                    if (PtAtAngle(splitPt).EqualsTol(tolLen, From) || PtAtAngle(splitPt).EqualsTol(tolLen, To)) continue;
                    hs_angles_rad.Add(splitPt.NormalizeAngle2PI());
                }

                var angles_rad = hs_angles_rad.OrderBy(w => w).ToList();
                if (!hs_angles_rad.Contains(AngleStartRad)) angles_rad.Insert(0, AngleStartRad);
                if (!hs_angles_rad.Contains(AngleEndRad)) angles_rad.Add(AngleEndRad);

                for (int i = 0; i < angles_rad.Count - 1; ++i)
                {
                    var arc = new Arc3D(CS, Radius, angles_rad[i], angles_rad[i + 1]);
                    yield return arc;
                }
            }

            /// <summary>
            /// intersect this 3d circle with given 3d line
            /// </summary>            
            public IEnumerable<Vector3D> Intersect(double tol, Line3D l, bool segment_mode = false)
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

            public Vector3D Center { get { return CS.Origin; } }

            /// <summary>
            /// centre of mass of circular segment
            /// </summary>            
            public Vector3D CentreOfMass(out double A)
            {
                // https://en.wikipedia.org/wiki/List_of_centroids

                var alpha = AngleRad / 2;
                A = Pow(Radius, 2) / 2 * (2 * alpha - Sin(2 * alpha));

                var x = (4 * Radius * Pow(Sin(alpha), 3)) / (3 * (2 * alpha - Sin(2 * alpha)));

                return (MidPoint - Center).Normalized() * x;
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

            public override BBox3D BBox(double tol_len, double tol_rad)
            {
                var pts = new List<Vector3D>() { From, To };

                foreach (var csdir in new[] { CS.BaseX, CS.BaseY })
                {
                    pts.AddRange(IntersectArc(tol_len, tol_rad,
                        new Line3D(CS.Origin, csdir, Line3DConstructMode.PointAndVector),
                        segment_mode: false,
                        arc_mode: true));
                }

                return new BBox3D(pts);
            }

            public IEnumerable<Vector3D> IntersectArc(double tol, double tolRad, Line3D l, bool segment_mode = false, bool arc_mode = true)
            {
                var q = Intersect(tol, l, segment_mode);
                if (q == null) yield break;

                q = q.Where(r => this.Contains(tol, tolRad, r)).ToList();
                if (q.Count() == 0) yield break;

                foreach (var x in q) yield return x;
            }

            public override string ToString()
            {
                return $"C:{Center} r:{Round(Radius, 3)} ANGLE:{Round(AngleRad.ToDeg(), 1)}deg FROM[{From} {Round(AngleStartRad.ToDeg(), 1)} deg] TO[{To} {Round(AngleEndRad.ToDeg(), 1)} deg]";
            }

            public override IEnumerable<Vector3D> Divide(int cnt, bool include_endpoints = false)
            {
                var from = GeomFrom;
                if (include_endpoints) yield return from;

                var p = from;
                var ang_step = AngleRad / cnt;

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
        }

    }

    public static partial class Extensions
    {

        public static Arc3D ToArc3D(this netDxf.Entities.Arc dxf_arc)
        {
            return new Arc3D(new CoordinateSystem3D(dxf_arc.Center, dxf_arc.Normal, CoordinateSystem3DAutoEnum.AAA), dxf_arc.Radius,
                dxf_arc.StartAngle.ToRad(), dxf_arc.EndAngle.ToRad());
        }

        /// <summary>
        /// states if given angle is contained in from, to angle range
        /// where angle_from assumed smaller angle
        /// and angle_to gets normalized taking in account 2*PI difference when angle_to &lt; angle_from
        /// </summary>        
        public static bool AngleInRange(this double pt_angle, double tol_rad, double angle_from, double angle_to)
        {
            pt_angle = pt_angle.NormalizeAngle2PI();

            if (pt_angle.LessThanTol(tol_rad, angle_from)) pt_angle += 2 * PI;

            var upper_bound_angle = angle_to;
            if (upper_bound_angle.LessThanTol(tol_rad, angle_from)) upper_bound_angle += 2 * PI;

            return pt_angle.GreatThanOrEqualsTol(tol_rad, angle_from) && pt_angle.LessThanOrEqualsTol(tol_rad, upper_bound_angle);
        }

    }

    public class Arc3DEqualityComparer : IEqualityComparer<Arc3D>
    {
        double tolLen;
        double tolRad;        

        public Arc3DEqualityComparer(double _tolLen, double _tolRad)
        {
            tolLen = _tolLen;
            tolRad = _tolRad;            
        }

        public bool Equals(Arc3D x, Arc3D y)
        {
            return x.EqualsTol(tolLen, tolRad, y);
        }

        public int GetHashCode(Arc3D obj)
        {
            return 0;
        }

    }

}

using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

using netDxf;
using ClipperLib;
using netDxf.Entities;
using static SearchAThing.SciToolkit;
using System.Text;

namespace SearchAThing
{

    public static partial class SciExt
    {

        /// <summary>
        /// (signed) Area of a polygon (does not consider z)
        /// https://en.wikipedia.org/wiki/Centroid        
        /// </summary>        
        public static double XYSignedArea(this IReadOnlyList<Vector3D> pts, double tol)
        {
            var lastEqualsFirst = pts[pts.Count - 1].EqualsTol(tol, pts[0]);
            double a = 0;

            for (int i = 0; i < pts.Count - 1; ++i)
                a += pts[i].X * pts[i + 1].Y - pts[i + 1].X * pts[i].Y;

            if (!lastEqualsFirst)
                a += pts[pts.Count - 1].X * pts[0].Y - pts[0].X * pts[pts.Count - 1].Y;

            return a / 2;
        }

        /// <summary>
        /// (abs) Area of a polygon (does not consider z)
        /// https://en.wikipedia.org/wiki/Centroid        
        /// </summary>        
        public static double XYArea(this IReadOnlyList<Vector3D> pts, double tol) => Math.Abs(XYSignedArea(pts, tol));

        /// <summary>
        /// Centroid of a polygon (does not consider z)    
        /// ( if have area specify the parameter to avoid recomputation )
        /// https://en.wikipedia.org/wiki/Centroid        
        /// </summary>        
        public static Vector3D XYCentroid(this IReadOnlyList<Vector3D> pts, double tol)
        {
            var signed_area = pts.XYSignedArea(tol);
            return pts.XYCentroid(tol, signed_area);
        }

        /// <summary>
        /// Centroid of a polygon (does not consider z)        
        /// https://en.wikipedia.org/wiki/Centroid        
        /// </summary>        
        public static Vector3D XYCentroid(this IReadOnlyList<Vector3D> pts, double tol, double signed_area)
        {
            var lastEqualsFirst = pts[pts.Count - 1].EqualsTol(tol, pts[0]);
            double x = 0;
            double y = 0;

            for (int i = 0; i < pts.Count - 1; ++i)
            {
                x += (pts[i].X + pts[i + 1].X) * (pts[i].X * pts[i + 1].Y - pts[i + 1].X * pts[i].Y);
                y += (pts[i].Y + pts[i + 1].Y) * (pts[i].X * pts[i + 1].Y - pts[i + 1].X * pts[i].Y);
            }

            if (!lastEqualsFirst)
            {
                x += (pts[pts.Count - 1].X + pts[0].X) * (pts[pts.Count - 1].X * pts[0].Y - pts[0].X * pts[pts.Count - 1].Y);
                y += (pts[pts.Count - 1].Y + pts[0].Y) * (pts[pts.Count - 1].X * pts[0].Y - pts[0].X * pts[pts.Count - 1].Y);
            }

            return new Vector3D(x / (6 * signed_area), y / (6 * signed_area), 0);
        }

        /// <summary>
        /// increase of decrease polygon points offseting
        /// ( this implementation uses Int64Map and clipper library )
        /// </summary>        
        public static IEnumerable<Vector3D> Offset(this IReadOnlyList<Vector3D> pts, double tol, double offset)
        {
            var intmap = new Int64Map(tol, pts.SelectMany(x => x.Coordinates));

            var clipper = new ClipperOffset();
            {
                var path = pts.Select(p => new IntPoint(intmap.ToInt64(p.X), intmap.ToInt64(p.Y))).ToList();
                // http://www.angusj.com/delphi/clipper.php
                clipper.AddPath(path, JoinType.jtMiter, EndType.etClosedPolygon);
            }

            var intoffset = intmap.ToInt64(intmap.Origin + offset) - intmap.ToInt64(intmap.Origin);

            var sol = new List<List<IntPoint>>();
            clipper.Execute(ref sol, intoffset);

            return sol.SelectMany(s => s.Select(si => new Vector3D(intmap.FromInt64(si.X), intmap.FromInt64(si.Y), 0)));
        }

        /// <summary>
        /// given a set of polygon pts, returns the enumeation of all pts
        /// so that the last not attach to the first ( if makeClosed = false ).
        /// Elsewhere it returns a last point equals the first ( makeClosed = true ).
        /// </summary>        
        public static IEnumerable<Vector3D> PolyPoints(this IEnumerable<Vector3D> pts, double tol, bool makeClosed = false)
        {
            Vector3D? first = null;

            var foundFirstAtend = false;

            foreach (var p in pts)
            {
                if (first == null)
                    first = p;
                else
                    if (first.EqualsTol(tol, p))
                {
                    if (makeClosed)
                        yield return p;
                    yield break;
                }

                yield return p;
            }

            if (first == null) throw new Exception($"empty pts input set");

            if (!foundFirstAtend && makeClosed)
                yield return first;
        }

        /// <summary>
        /// yields an ienumerable of polygon segments corresponding to the given polygon pts ( z is not considered )
        /// works even last point not equals the first one
        /// </summary>       
        public static IEnumerable<Line3D> PolygonSegments(this IEnumerable<Vector3D> pts, double tol)
        {
            Vector3D? first = null;
            Vector3D? prev = null;

            foreach (var p in pts)
            {
                if (first == null)
                {
                    first = prev = p;
                    continue;
                }

                var seg = new Line3D(prev!, p);
                prev = p;

                yield return seg;
            }

            if (prev == null) yield break;

            if (first == null) throw new Exception($"empty pts input set");

            if (!prev.EqualsTol(tol, first)) yield return new Line3D(prev, first);
        }

        /// <summary>
        /// states if given point is in polygon
        /// </summary>
        /// <param name="_pts">polygon point ( must ordered )</param>
        /// <param name="tol">length tolerance</param>
        /// <param name="pt">point to test</param>        
        /// <param name="mode">allow to specify contains test type</param>        
        public static bool ContainsPoint(this IReadOnlyList<Vector3D> _pts, double tol, Vector3D pt,
            LoopContainsPointMode mode = LoopContainsPointMode.InsideOrPerimeter) =>
            new Loop(tol, _pts.RepeatFirstAtEnd(tol).Segments(tol))
            .ContainsPoint(tol, pt, mode);

        public static IEnumerable<Vector3D> SortPoly(this IEnumerable<Vector3D> pts, double tol, Vector3D? refAxis = null) =>
            pts.SortPoly(tol, (p) => p, refAxis);

        /// <summary>
        /// Sort polygon segments so that they can form a polygon ( if they really form one ).
        /// It will not check for segment versus adjancency
        /// </summary>        
        public static IEnumerable<Line3D> SortPoly(this IEnumerable<Line3D> segs, double tol, Vector3D? refAxis = null) =>
            segs.SortPoly(tol, (s) => s.MidPoint, refAxis);

        public static IEnumerable<T> SortPoly<T>(this IEnumerable<T> pts, double tol, Func<T, Vector3D> getPoint, Vector3D? refAxis = null)
        {
            if (pts.Count() == 1) return pts;

            var c = pts.Select(w => getPoint(w)).Mean();

            var r = getPoint(pts.First()) - c;

            // search non-null ref axis
            Vector3D? N = null;
            if (refAxis != null)
                N = refAxis;
            else
            {
                foreach (var r2 in pts.Skip(1))
                {
                    N = r.CrossProduct(getPoint(r2) - c);
                    if (!N.Length.EqualsTol(tol, 0)) break;
                }
            }

            if (N == null) throw new Exception($"can't state normal from pts input set");

            var q = pts.Select(p => new
            {
                pt = p,
                ang = r.AngleToward(tol, getPoint(p) - c, N)
            });
            var res = q.OrderBy(w => w.ang).Select(w => w.pt);

            return res;
        }

        /// <summary>
        /// Return the input set of segments until an adjacency between one and next is found.
        /// It can rectify the versus of line (by default) if needed.
        /// Note: returned set references can be different if rectifyVersus==true
        /// </summary>        
        public static IEnumerable<Line3D> TakeUntilAdjacent(this IEnumerable<Line3D> segs, double tol, bool rectifyVersus = true)
        {
            var prev = segs.First();
            var iter = 0;

            foreach (var cur in segs.Skip(1))
            {
                ++iter;
                if (cur.From.EqualsTol(tol, prev.To))
                {
                    if (iter == 1) yield return prev;
                    yield return cur;
                    prev = cur;
                    continue;
                }

                // take a change to check versus
                if (rectifyVersus)
                {
                    // cur need to be reversed
                    if (cur.To.EqualsTol(tol, prev.To))
                    {
                        if (iter == 1) yield return prev;
                        prev = cur.Reverse();
                        yield return prev;
                        continue;
                    }

                    // take a change to swap the first segment
                    if (iter == 1)
                    {
                        if (prev.From.EqualsTol(tol, cur.From))
                        {
                            yield return prev.Reverse();
                            yield return cur;
                            prev = cur;
                            continue;
                        }
                        if (prev.From.EqualsTol(tol, cur.To))
                        {
                            yield return prev.Reverse();
                            prev = cur.Reverse();
                            yield return prev;
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Preprocess segs with SortPoly if needed.
        /// Return the ordered segments poly or null if not a closed poly.
        /// </summary>        
        public static IEnumerable<Line3D>? IsAClosedPoly(this IEnumerable<Line3D> segs, double tol)
        {
            var sply = segs.SortPoly(tol).TakeUntilAdjacent(tol).ToList();

            if (sply.Count != segs.Count()) return null;

            if (!sply.First().From.EqualsTol(tol, sply.Last().To)) return null;

            return sply;
        }

        /// <summary>
        /// Find intersection points (0,1,2) of the given line with the given polygon
        /// TODO unit test
        /// </summary>        
        public static IEnumerable<Vector3D> Intersect(this IEnumerable<Line3D> polygonSegments,
            double tol, Line3D line, GeomSegmentMode segmentMode)
        {
            var lineDir = line.To - line.From;
            foreach (var s in polygonSegments)
            {
                var i = s.Intersect(tol, line, true, false);
                if (i == null) continue;
                switch (segmentMode)
                {
                    case GeomSegmentMode.Infinite: yield return i; break;
                    case GeomSegmentMode.From: if ((i - line.From).Concordant(tol, lineDir)) yield return i; break;
                    case GeomSegmentMode.To: if (!(i - line.To).Concordant(tol, lineDir)) yield return i; break;
                    case GeomSegmentMode.FromTo: if (line.SegmentContainsPoint(tol, i)) yield return i; break;
                }
            }

            yield break;
        }

        public static netDxf.Entities.Hatch ToHatch(this Polyline2D lw,
          HatchPattern pattern, bool associative = true)
        {
            var hatch = new netDxf.Entities.Hatch(pattern, new[] { new HatchBoundaryPath(new[] { lw }) }, associative);
            hatch.Normal = lw.Normal;
            hatch.Elevation = lw.Elevation;
            return hatch;
        }

        public static netDxf.Entities.Hatch ToHatch(this IEnumerable<Geometry> _geom,
            HatchPattern pattern, bool associative = true) =>
            new netDxf.Entities.Hatch(pattern, new[] { new HatchBoundaryPath(_geom.Select(w => w.DxfEntity)) }, associative);

        /// <summary>
        /// qcad script from geoms
        /// </summary>        
        public static string QCadScript(this IEnumerable<Geometry> geoms)
        {
            var sb = new StringBuilder();

            IReadOnlyList<Geometry> geomsLst;

            if (geoms is IReadOnlyList<Geometry>)
                geomsLst = (IReadOnlyList<Geometry>)geoms;
            else
                geomsLst = geoms.ToList();

            for (int i = 0; i < geomsLst.Count; ++i)
            {
                var geom = geomsLst[i];

                switch (geom.GeomType)
                {
                    case GeometryType.Vector3D:
                        sb.AppendLine(((Vector3D)geom).QCadScript(final: i == geomsLst.Count - 1));
                        break;

                    case GeometryType.Line3D:
                        sb.AppendLine(((Line3D)geom).QCadScript(final: i == geomsLst.Count - 1));
                        break;

                    case GeometryType.Arc3D:
                        sb.AppendLine(((Arc3D)geom).QCadScript(final: i == geomsLst.Count - 1));
                        break;

                    case GeometryType.Circle3D:
                        sb.AppendLine(((Circle3D)geom).QCadScript(final: i == geomsLst.Count - 1));
                        break;

                    default: throw new NotImplementedException($"entity type {geom.GeomType}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// qcad script from lwp
        /// </summary>        
        public static string QCadScript(this netDxf.Entities.Polyline2D lwp, double tol) =>
            lwp.ToGeometries(tol).QCadScript();

        /// <summary>
        /// build 2d dxf polyline.
        /// precondition: geom vertex must lie on the same plane
        /// note: use RepeatFirstAtEnd extension to build a closed polyline
        /// </summary>
        /// <param name="_geom"></param>
        /// <param name="tol">length tolerance</param>
        /// <param name="cs">lw CS</param>
        /// <param name="closed"></param>        
        /// <returns></returns>
        public static netDxf.Entities.Polyline2D ToLwPolyline(this IEnumerable<Geometry> _geom, double tol,
            CoordinateSystem3D cs, bool closed)
        {
            var edges = _geom.Cast<Edge>().ToList();

            var elevation = cs.Origin.ToUCS(cs, evalCSOrigin: false).Z;

            if (cs.csAutoType != CoordinateSystem3DAutoEnum.AAA)
            {
                var originBk = cs.Origin;

                cs = new CoordinateSystem3D(Vector3D.Zero, cs.BaseZ, CoordinateSystem3DAutoEnum.AAA);
            }

            var pvtx = new List<Polyline2DVertex>();

            Vector3D? _lastPt = null;

            for (int i = 0; i < edges.Count; ++i)
            {
                var _from = edges[i].SGeomFrom.ToUCS(cs);
                var _to = edges[i].SGeomTo.ToUCS(cs);

                switch (edges[i].GeomType)
                {
                    case GeometryType.Vector3D:
                        {
                            var lwpv = new Polyline2DVertex(_to.ToDxfVector2());
                            pvtx.Add(lwpv);
                            _lastPt = _to;
                        }
                        break;

                    case GeometryType.Line3D:
                        {
                            if (_lastPt == null || _lastPt.EqualsTol(tol, _from))
                            {
                                var lwpv = new Polyline2DVertex(_from.ToDxfVector2());
                                pvtx.Add(lwpv);
                                _lastPt = _to;
                            }
                            else
                            {
                                var lwpv = new Polyline2DVertex(_to.ToDxfVector2());
                                pvtx.Add(lwpv);
                                _lastPt = _from;
                            }
                        }
                        break;

                    case GeometryType.Arc3D:
                        {
                            var arc = (Arc3D)edges[i];
                            var bulge = arc.Bulge(tol);
                            if (cs.BaseZ.EqualsTol(NormalizedLengthTolerance, -arc.CS.BaseZ)) bulge *= -1d;

                            if (_lastPt == null)
                            {
                                if (i < edges.Count - 1)
                                {
                                    if (edges[i + 1].SGeomFrom.EqualsTol(tol, _to))
                                    {
                                        var lwpv = new Polyline2DVertex(_from.ToDxfVector2()) { Bulge = bulge };
                                        pvtx.Add(lwpv);
                                        _lastPt = _to;
                                    }
                                    else
                                    {
                                        var lwpv = new Polyline2DVertex(_to.ToDxfVector2()) { Bulge = -bulge };
                                        pvtx.Add(lwpv);
                                        _lastPt = _from;
                                    }
                                }
                                else
                                {
                                    var lwpv = new Polyline2DVertex(_from.ToDxfVector2()) { Bulge = bulge };
                                    pvtx.Add(lwpv);
                                    _lastPt = _to;
                                }
                            }
                            else
                            {
                                if (_lastPt.EqualsTol(tol, _from))
                                {
                                    var lwpv = new Polyline2DVertex(_from.ToDxfVector2()) { Bulge = bulge };
                                    pvtx.Add(lwpv);
                                    _lastPt = _to;
                                }
                                else
                                {
                                    var lwpv = new Polyline2DVertex(_to.ToDxfVector2()) { Bulge = -bulge };
                                    pvtx.Add(lwpv);
                                    _lastPt = _from;
                                }
                            }

                        }
                        break;
                }
            }

            if (!closed)
            {
                if (_lastPt == null) throw new ArgumentException("can't find last pt");
                var lwpv = new Polyline2DVertex(_lastPt.ToDxfVector2());
                pvtx.Add(lwpv);
            }

            var lwpoly = new Polyline2D(pvtx, isClosed: closed);

            lwpoly.Normal = cs.BaseZ;
            lwpoly.Elevation = elevation;

            return lwpoly;
        }

        /// <summary>
        /// build 3d dxf polyline
        /// note: use RepeatFirstAtEnd extension to build a closed polyline
        /// </summary>        
        public static Polyline3D ToPolyline(this IEnumerable<Vector3D> pts, bool isClosed = true) =>
            new Polyline3D(pts.Select(r => (Vector3)r).ToList(), isClosed);

        /// <summary>
        /// can generate a Int64MapExceptionRange exception if double values can't fit into a In64 representation.
        /// In that case try with tolerances not too small.
        /// It is suggested to use a lenTol/10 to avoid lost of precision during domain conversions.
        /// Altenatively use Loop to find exact intersection between planar poly supporting lines and arcs.
        /// ( this implementation uses Int64Map and clipper library )
        /// </summary>        
        public static IEnumerable<IEnumerable<Vector3D>> Boolean(this IEnumerable<Vector3D> polyA, double tol, IEnumerable<Vector3D> polyB, ClipType type, bool selfCheckInt64MapTolerance = true)
        {
            var intmap = new Int64Map(tol, polyA.SelectMany(x => x.Coordinates).Union(polyB.SelectMany(x => x.Coordinates)), selfCheckInt64MapTolerance);

            var clipper = new Clipper();
            {
                var path = polyA.Select(p => new IntPoint(intmap.ToInt64(p.X), intmap.ToInt64(p.Y))).ToList();
                clipper.AddPath(path, PolyType.ptSubject, true);
            }
            {
                var path = polyB.Select(p => new IntPoint(intmap.ToInt64(p.X), intmap.ToInt64(p.Y))).ToList();
                clipper.AddPath(path, PolyType.ptClip, true);
            }

            var sol = new List<List<IntPoint>>();
            clipper.Execute(type, sol);

            var res = sol.Select(s => s.Select(si => new Vector3D(intmap.FromInt64(si.X), intmap.FromInt64(si.Y), 0)));

            return res;
        }

        /// <summary>
        /// compute convex hull using LoycCore
        /// https://github.com/qwertie/LoycCore
        /// </summary>        
        public static IEnumerable<Vector3D> ConvexHull2D(this IEnumerable<Vector3D> pts)
        {
            throw new NotImplementedException();
            /*
            return Loyc.Geometry.PointMath.ComputeConvexHull(pts.Select(w => new Loyc.Geometry.Point<double>(w.X, w.Y)))
                .Select(w => new Vector3D(w.X, w.Y));*/
        }

        /// <summary>
        /// tessellate given pts list using 1 contour in clockwise ordering
        /// see used tessellation library ( https://github.com/speps/LibTessDotNet )
        /// </summary>
        /// <param name="pts">pts to tessellate in triangles</param>
        /// <returns>list of triangles</returns>
        public static IEnumerable<Triangle3D> Tessellate(this IReadOnlyList<Vector3D> pts)
        {
            var tess = new LibTessDotNet.Tess();

            var contour = new LibTessDotNet.ContourVertex[pts.Count];

            for (int i = 0; i < pts.Count; ++i)
            {
                contour[i].Position = new LibTessDotNet.Vec3((float)pts[i].X, (float)pts[i].Y, (float)pts[i].Z);
            }

            tess.AddContour(contour, LibTessDotNet.ContourOrientation.Clockwise);

            tess.Tessellate();

            for (int i = 0; i < tess.ElementCount; ++i)
            {
                yield return new Triangle3D
                {
                    a = tess.Vertices[tess.Elements[i * 3]].Position,
                    b = tess.Vertices[tess.Elements[i * 3 + 1]].Position,
                    c = tess.Vertices[tess.Elements[i * 3 + 2]].Position
                };
            }

        }

        public static Triangle3D ToTriangle3D(this IEnumerable<Vector3D> pts)
        {
            var res = new Triangle3D();

            int idx = 0;
            foreach (var p in pts)
            {
                switch (idx)
                {
                    case 0: res.a = p; break;
                    case 1: res.b = p; break;
                    case 2: res.c = p; break;
                    default: throw new ArgumentException($"need exactly 3 pts for triangle");
                }
                ++idx;
            }

            return res;
        }

    }

    public static partial class SciToolkit
    {

        /// <summary>
        /// create an approximation of ellipse
        /// </summary>
        /// <param name="center">center of ellipse</param>
        /// <param name="width">width of ellipse</param>
        /// <param name="height">height of ellipse</param>
        /// <param name="flatness">maximum error of approximation</param>
        /// <returns>vertexes of approximated ellipse (including last=first)</returns>
        public static IEnumerable<Vector3D> EllipseToPolygon2D(Vector3D center, double width, double height, double flatness = .1)
        {
            var _center = Vector3D.Zero;

            var gp = new GraphicsPath();
            gp.AddEllipse((float)(_center.X - width / 2), (float)(_center.Y - height / 2), (float)width, (float)height);
            gp.Flatten(new Matrix(), (float)flatness);
            foreach (var p in gp.PathPoints) yield return new Vector3D(p.X, p.Y, 0) + center;
        }

    }

}
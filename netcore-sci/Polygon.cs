using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using SearchAThing.Sci;
using System.Drawing.Drawing2D;
using netDxf;
using SearchAThing;
using ClipperLib;
using SearchAThing.Util;

namespace SearchAThing
{

    namespace Sci
    {

        public static class Polygon
        {

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

    public static partial class Extensions
    {

        /// <summary>
        /// Area of a polygon (does not consider z)
        /// https://en.wikipedia.org/wiki/Centroid        
        /// </summary>        
        public static double Area(this IReadOnlyList<Vector3D> pts, double tol)
        {
            var lastEqualsFirst = pts[pts.Count - 1].EqualsTol(tol, pts[0]);
            double a = 0;

            for (int i = 0; i < pts.Count - 1; ++i)
                a += pts[i].X * pts[i + 1].Y - pts[i + 1].X * pts[i].Y;

            if (!lastEqualsFirst)
                a += pts[pts.Count - 1].X * pts[0].Y - pts[0].X * pts[pts.Count - 1].Y;

            return Abs(a / 2);
        }

        /// <summary>
        /// Centroid of a polygon (does not consider z)
        /// note: points must ordered anticlockwise
        /// ( if have area specify the parameter to avoid recomputation )
        /// https://en.wikipedia.org/wiki/Centroid        
        /// </summary>        
        public static Vector3D Centroid(this IReadOnlyList<Vector3D> pts, double tol)
        {
            var area = pts.Area(tol);
            return pts.Centroid(tol, area);
        }

        /// <summary>
        /// Centroid of a polygon (does not consider z)
        /// note: points must ordered anticlockwise
        /// https://en.wikipedia.org/wiki/Centroid        
        /// </summary>        
        public static Vector3D Centroid(this IReadOnlyList<Vector3D> pts, double tol, double area)
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

            return new Vector3D(x / (6 * area), y / (6 * area), 0);
        }

        /// <summary>
        /// increase of decrease polygon points offseting
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
            Vector3D first = null;

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

            if (!foundFirstAtend && makeClosed)
                yield return first;
        }

        /// <summary>
        /// yields an ienumerable of polygon segments corresponding to the given polygon pts ( z is not considered )
        /// works even last point not equals the first one
        /// </summary>       
        public static IEnumerable<Line3D> PolygonSegments(this IEnumerable<Vector3D> pts, double tol)
        {
            Vector3D first = null;
            Vector3D prev = null;

            foreach (var p in pts)
            {
                if (first == null)
                {
                    first = prev = p;
                    continue;
                }

                var seg = new Line3D(prev, p);
                prev = p;

                yield return seg;
            }

            if (prev == null) throw new ArgumentException("empty set given");
            if (!prev.EqualsTol(tol, first)) yield return new Line3D(prev, first);
        }

        /// <summary>        
        /// states if the given polygon contains the test point ( z not considered )
        /// https://en.wikipedia.org/wiki/Point_in_polygon
        /// By default check the point contained in the polygon perimeter.
        /// Optionally duplicate points are zapped in comparing.
        /// </summary>                
        public static bool ContainsPoint(this IReadOnlyList<Vector3D> _pts, double tol, Vector3D _pt, bool zapDuplicates = false)
        {
            var pt = _pt.Set(OrdIdx.Z, 0);
            var pts = _pts.Select(w => w.Set(OrdIdx.Z, 0));
            var ptHs = new HashSet<Vector3D>(new Vector3DEqualityComparer(tol));
            var ptsFiltered = pts;

            var pts_bbox = _pts.BBox();
            if (!pts_bbox.Contains2D(tol, _pt)) return false;

            if (zapDuplicates)
            {
                var tmp = new List<Vector3D>();
                foreach (var p in pts)
                {
                    if (!ptHs.Contains(p))
                    {
                        ptHs.Add(p);
                        tmp.Add(p);
                    }
                }
                ptsFiltered = tmp;
            }
            var segs = ptsFiltered.PolygonSegments(tol);

            if (_pts.Any(w => _pt.EqualsTol(tol, w))) return true;

            var ray = new Line3D(pt, Vector3D.XAxis, Line3DConstructMode.PointAndVector);
            var conflictVertex = false;
            do
            {
                conflictVertex = false;
                foreach (var pp in ptsFiltered)
                {
                    if (ray.LineContainsPoint(tol, pp))
                    {
                        conflictVertex = true;
                        // ray intersect vertex, change it
                        ray = new Line3D(pt, pp + Vector3D.YAxis * tol * 1.1);
                        break;
                    }
                }
            }
            while (conflictVertex);

            var intCnt = 0;

            foreach (var seg in segs)
            {
                if (seg.SegmentContainsPoint(tol, pt)) return true;

                Vector3D ip = null;

                ip = ray.Intersect(tol, seg);
                if (ip != null && seg.SegmentContainsPoint(tol, ip))
                {
                    if (pt.X.GreatThanOrEqualsTol(tol, ip.X))
                        ++intCnt;
                }
            }

            return intCnt % 2 != 0;
        }

        public static IEnumerable<Vector3D> SortPoly(this IEnumerable<Vector3D> pts, double tol, Vector3D refAxis = null)
        {
            return pts.SortPoly(tol, (p) => p, refAxis);
        }

        /// <summary>
        /// Sort polygon segments so that they can form a polygon ( if they really form one ).
        /// It will not check for segment versus adjancency
        /// </summary>        
        public static IEnumerable<Line3D> SortPoly(this IEnumerable<Line3D> segs, double tol, Vector3D refAxis = null)
        {
            return segs.SortPoly(tol, (s) => s.MidPoint, refAxis);
        }

        public static IEnumerable<T> SortPoly<T>(this IEnumerable<T> pts, double tol, Func<T, Vector3D> getPoint, Vector3D refAxis = null)
        {
            if (pts.Count() == 1) return pts;

            var c = pts.Select(w => getPoint(w)).Mean();

            var r = getPoint(pts.First()) - c;

            // search non-null ref axis
            Vector3D N = null;
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
        public static IEnumerable<Line3D> IsAClosedPoly(this IEnumerable<Line3D> segs, double tol)
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
            double tol, Line3D line, Line3DSegmentMode segmentMode)
        {
            var lineDir = line.To - line.From;
            foreach (var s in polygonSegments)
            {
                var i = s.Intersect(tol, line, true, false);
                if (i == null) continue;
                switch (segmentMode)
                {
                    case Line3DSegmentMode.None: yield return i; break;
                    case Line3DSegmentMode.From: if ((i - line.From).Concordant(tol, lineDir)) yield return i; break;
                    case Line3DSegmentMode.To: if (!(i - line.To).Concordant(tol, lineDir)) yield return i; break;
                    case Line3DSegmentMode.FromTo: if (line.SegmentContainsPoint(tol, i)) yield return i; break;
                }
            }

            yield break;
        }

        /// <summary>
        /// build 2d dxf polyline.
        /// note: use RepeatFirstAtEnd extension to build a closed polyline
        /// </summary>        
        public static netDxf.Entities.LwPolyline ToLwPolyline(this IEnumerable<Geometry> _geom, double tolLen, bool closed = true)
        {
            var geom = _geom.ToList();

            var N = Vector3D.ZAxis;

            var cs = new CoordinateSystem3D(Vector3D.Zero, N, CoordinateSystem3DAutoEnum.AAA);

            var pvtx = new List<netDxf.Entities.LwPolylineVertex>();

            Vector3D lastPt = null;

            for (int i = 0; i < geom.Count; ++i)
            {
                Vector3D from = null;
                Vector3D to = null;

                switch (geom[i].Type)
                {
                    case GeometryType.Vector3D:
                        {
                            to = geom[i] as Vector3D;
                            var lwpv = new netDxf.Entities.LwPolylineVertex(to.ToVector2());
                            pvtx.Add(lwpv);
                            lastPt = to;
                        }
                        break;

                    case GeometryType.Line3D:
                        {
                            var seg = geom[i] as Line3D;
                            from = seg.From;
                            to = seg.To;

                            if (lastPt == null || lastPt.EqualsTol(tolLen, from))
                            {
                                var lwpv = new netDxf.Entities.LwPolylineVertex(from.ToUCS(cs).ToVector2());
                                pvtx.Add(lwpv);
                                lastPt = to;
                            }
                            else
                            {
                                var lwpv = new netDxf.Entities.LwPolylineVertex(to.ToUCS(cs).ToVector2());
                                pvtx.Add(lwpv);
                                lastPt = from;
                            }
                        }
                        break;
                    case GeometryType.Arc3D:
                        {
                            var arc = geom[i] as Arc3D;
                            from = arc.From;
                            to = arc.To;
                            var bulge = arc.Bulge(tolLen, arc.From, arc.To, N);

                            if (lastPt == null)
                            {
                                if (i < geom.Count - 1)
                                {
                                    if (geom[i + 1].GeomFrom.EqualsTol(tolLen, to))
                                    {
                                        var lwpv = new netDxf.Entities.LwPolylineVertex(from.ToUCS(cs).ToVector2()) { Bulge = bulge };
                                        pvtx.Add(lwpv);
                                        lastPt = to;
                                    }
                                    else
                                    {
                                        var lwpv = new netDxf.Entities.LwPolylineVertex(to.ToUCS(cs).ToVector2()) { Bulge = -bulge };
                                        pvtx.Add(lwpv);
                                        lastPt = from;
                                    }
                                }
                                else
                                {
                                    var lwpv = new netDxf.Entities.LwPolylineVertex(from.ToUCS(cs).ToVector2()) { Bulge = bulge };
                                    pvtx.Add(lwpv);
                                    lastPt = to;
                                }
                            }
                            else
                            {
                                if (lastPt.EqualsTol(tolLen, from))
                                {
                                    var lwpv = new netDxf.Entities.LwPolylineVertex(from.ToUCS(cs).ToVector2()) { Bulge = bulge };
                                    pvtx.Add(lwpv);
                                    lastPt = to;
                                }
                                else
                                {
                                    var lwpv = new netDxf.Entities.LwPolylineVertex(to.ToUCS(cs).ToVector2()) { Bulge = -bulge };
                                    pvtx.Add(lwpv);
                                    lastPt = from;
                                }
                            }

                        }
                        break;
                }
            }

            if (!closed)
            {
                if (lastPt==null) throw new Exception("can't find last pt");
                var lwpv = new netDxf.Entities.LwPolylineVertex(lastPt.ToUCS(cs).ToVector2());
                pvtx.Add(lwpv);
            }

            var lwpoly = new netDxf.Entities.LwPolyline(pvtx, isClosed: closed);

            lwpoly.Normal = N.Normalized();

            return lwpoly;
        }

        /// <summary>
        /// build 3d dxf polyline
        /// note: use RepeatFirstAtEnd extension to build a closed polyline
        /// </summary>        
        public static netDxf.Entities.Polyline ToPolyline(this IEnumerable<Vector3D> pts, bool isClosed = true)
        {
            return new netDxf.Entities.Polyline(pts.Select(r => (Vector3)r).ToList(), isClosed);
        }

        /// <summary>
        /// can generate a Int64MapExceptionRange exception if double values can't fit into a In64 representation.
        /// In that case try with tolerances not too small.
        /// It is suggested to use a lenTol/10 to avoid lost of precision during domain conversions.
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

    }

}

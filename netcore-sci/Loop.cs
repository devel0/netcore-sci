using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using netDxf;
using ClipperLib;
using netDxf.Entities;

using SearchAThing;
using System.Runtime.CompilerServices;

namespace SearchAThing
{

    /// <summary>
    /// planar edges loop
    /// </summary>
    public class Loop
    {

        public Plane3D Plane { get; private set; }

        public List<IEdge> Edges { get; private set; }

        public double Tol { get; private set; }

        public Loop(double tol, IEnumerable<IEdge> edges, bool checkSense = true)
        {
            Tol = tol;
            Edges = checkSense ? edges.CheckSense(tol).ToList() : edges.ToList();
            Plane = Edges.DetectPlane(tol);
        }

        public Loop(double tol, LwPolyline lwPolyline)
        {
            Tol = tol;
            Edges = lwPolyline.ToGeometries(tol).Cast<IEdge>().CheckSense(tol).ToList();
            Plane = Edges.DetectPlane(tol);
        }

        double? _Area = null;

        /// <summary>
        /// area of the loop ( segment and arc are evaluated )
        /// </summary>        
        public double Area
        {
            get
            {
                if (_Area == null) _Area = ComputeArea(Tol);

                return _Area.Value;
            }
        }

        double ComputeArea(double tol)
        {
            var polysegs = Edges.Select(w => w.SGeomFrom).ToList();
            var res = polysegs.Area(tol);

            foreach (var edge in Edges.Where(r => r.EdgeType == EdgeType.Arc3D))
            {
                var arc = (Arc3D)edge;

                var midpt = edge.MidPoint;
                if (polysegs.ContainsPoint(tol, midpt))
                    res -= arc.SegmentArea;
                else
                    res += arc.SegmentArea;
            }

            return res;
        }

        public bool ContainsPoint(double tol, Vector3D pt)
        {
            if (this.Edges.Any(edge => edge.EdgeContainsPoint(tol, pt))) return true;

            var ray = pt.LineV(Plane.CS.BaseX);

            var qits = this.Edges.Cast<Geometry>().Intersect(tol, new[] { ray }).ToList();

            if (qits.Count == 0) return false;

            var qips = qits.Where(r => r.GeomType == GeometryType.Vector3D).Select(w => (Vector3D)w).ToList();

            var rayVNorm = ray.V.Normalized();
            var sortedColinearPts = qips
                .Select(ip => new
                {
                    ip,
                    off = ip.ColinearScalarOffset(tol, ray.From, rayVNorm)
                })
                .OrderBy(w => w.off)
                .Where(w => w.off.GreatThanOrEqualsTol(tol, 0))
                .Select(w => w.ip)
                .ToList();

            return sortedColinearPts.Count % 2 != 0;
        }

        record struct GeomNfo(IEdge geom, bool inside);

        record struct GeomWithIdx(IEdge geom, int idx);

        record struct GeomWalkNfo(List<GeomNfo> lst, bool isOnThis, IEdge geom, int geomIdx,
            HashSet<IEdge> geomVisited, HashSet<Vector3D> vertexVisited);

        public IEnumerable<Loop> Intersect(double tol, Loop other)
        {
            var res = new List<List<IEdge>>();

            List<GeomNfo>? thisBrokenGeoms = null;
            List<GeomNfo>? otherBrokenGeoms = null;
            List<Vector3D>? ipts = null;

            {
                var thisGeoms = Edges.ToList();
                var otherGeoms = other.Edges.ToList();

                var combs = from tg in thisGeoms
                            from og in otherGeoms
                            select new { tg, og };

                var iptsNfos = combs.SelectMany(comb =>
                    ((Geometry)comb.tg).GeomIntersect(tol, (Geometry)comb.og)
                    .Select(x => x.GeomType == GeometryType.Vector3D ? x :
                        throw new NotImplementedException($"intersect implements only pts, but got {x.GeomType}"))
                    .Select(i_nfo => new
                    {
                        igeom = i_nfo,
                        tent = comb.tg,
                        oent = comb.og
                    })
                ).ToList();

                if (iptsNfos.Count == 0)
                {
                    var opts = other.Edges.Select(edge => (Vector3D)edge.SGeomFrom).ToList();
                    if (opts.All(opt => this.ContainsPoint(tol, opt)))
                        yield return other;

                    var tpts = this.Edges.Select(edge => (Vector3D)edge.SGeomFrom).ToList();
                    if (tpts.All(opt => other.ContainsPoint(tol, opt)))
                        yield return this;

                    yield break;
                }

                var tgeomsBreaks = iptsNfos
                    .GroupBy(w => w.tent)
                    .Select(w => new { ent = w.Key, breaks = w.Select(u => (Vector3D)u.igeom).ToList() })
                    .ToDictionary(k => k.ent, v => v.breaks);

                var ogeomsBreaks = iptsNfos
                    .GroupBy(w => w.oent)
                    .Select(w => new { ent = w.Key, breaks = w.Select(u => (Vector3D)u.igeom).ToList() })
                    .ToDictionary(k => k.ent, v => v.breaks);

                thisBrokenGeoms = thisGeoms.SelectMany(tgeom =>
                {
                    if (tgeomsBreaks.TryGetValue(tgeom, out var ipts))
                    {
                        var res = tgeom.Split(tol, ipts)
                            .Select(geom =>
                            {
                                var res = new GeomNfo((IEdge)geom, inside: other.ContainsPoint(tol, geom.MidPoint));
                                return res;
                            }).ToList();

                        return res.ToArray();
                    }
                    else
                        return new[] { new GeomNfo((IEdge)tgeom, inside: other.ContainsPoint(tol, tgeom.MidPoint)) };
                }).ToList();

                otherBrokenGeoms = otherGeoms.SelectMany(ogeom =>
                {
                    if (ogeomsBreaks.TryGetValue(ogeom, out var ipts))
                        return ogeom.Split(tol, ipts)
                            .Select(geom =>
                            {
                                var res = new GeomNfo((IEdge)geom, inside: this.ContainsPoint(tol, geom.MidPoint));
                                return res;
                            });
                    else
                        return new[] { new GeomNfo((IEdge)ogeom, inside: this.ContainsPoint(tol, ogeom.MidPoint)) };
                })
                .ToList();

                ipts = iptsNfos.Select(w => (Vector3D)w.igeom).ToList();
            }

            var ptCmp = new Vector3DEqualityComparer(tol);
            var iptsHs = ipts.ToHashSet(ptCmp);

            var ipToThisBrokenGeoms = new Dictionary<Vector3D, GeomWithIdx>(ptCmp);
            foreach (var nfo in thisBrokenGeoms.WithIndex())
            {
                var containsFrom = iptsHs.Contains(nfo.item.geom.SGeomFrom, ptCmp);
                var containsTo = iptsHs.Contains(nfo.item.geom.SGeomTo, ptCmp);
                if (nfo.item.inside)
                {
                    if (containsFrom)
                        ipToThisBrokenGeoms.Add(nfo.item.geom.SGeomFrom, new GeomWithIdx(nfo.item.geom, nfo.idx));

                    if (containsTo)
                        ipToThisBrokenGeoms.Add(nfo.item.geom.SGeomTo, new GeomWithIdx(nfo.item.geom, nfo.idx));
                }
            }

            var ipToOtherBrokenGeoms = new Dictionary<Vector3D, GeomWithIdx>(ptCmp);
            foreach (var nfo in otherBrokenGeoms.WithIndex())
            {
                var containsFrom = iptsHs.Contains(nfo.item.geom.SGeomFrom, ptCmp);
                var containsTo = iptsHs.Contains(nfo.item.geom.SGeomTo, ptCmp);
                if (nfo.item.inside)
                {
                    if (containsFrom)
                        ipToOtherBrokenGeoms.Add(nfo.item.geom.SGeomFrom, new GeomWithIdx(nfo.item.geom, nfo.idx));

                    if (containsTo)
                        ipToOtherBrokenGeoms.Add(nfo.item.geom.SGeomTo, new GeomWithIdx(nfo.item.geom, nfo.idx));
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            int calcIdx(int curIdx, int dir, int N)
            {
                if (dir > 0)
                    return curIdx == N - 1 ? 0 : curIdx + 1;
                else
                    return curIdx == 0 ? N - 1 : curIdx - 1;
            }

            bool fromOrToEquals(Geometry geom, Vector3D p) =>
                geom.GeomFrom.EqualsTol(tol, p) || geom.GeomTo.EqualsTol(tol, p);

            var thisGeomVisited = new HashSet<IEdge>();
            var thisVertexVisited = new HashSet<Vector3D>(ptCmp);

            var otherGeomVisited = new HashSet<IEdge>();
            var otherVertexVisited = new HashSet<Vector3D>(ptCmp);

            GeomWalkNfo getByIp(Vector3D ip, bool onThis)
            {
                var lst = onThis ? thisBrokenGeoms : otherBrokenGeoms;
                var q = onThis ? ipToThisBrokenGeoms[ip] : ipToOtherBrokenGeoms[ip];

                var geomVisited = onThis ? thisGeomVisited : otherGeomVisited;
                var vertexVisited = onThis ? thisVertexVisited : otherVertexVisited;

                geomVisited.Add(q.geom);
                vertexVisited.Add(ip);

                return new GeomWalkNfo(lst, isOnThis: onThis, q.geom, q.idx, geomVisited, vertexVisited);
            }

            Vector3D? sharedVertex(IEdge g1, IEdge g2)
            {
                if (g1.SGeomFrom.EqualsTol(tol, g2.SGeomFrom)
                    ||
                    (g1.SGeomFrom.EqualsTol(tol, g2.SGeomTo)))
                    return g1.SGeomFrom;

                if (g1.SGeomTo.EqualsTol(tol, g2.SGeomFrom)
                    ||
                    (g1.SGeomTo.EqualsTol(tol, g2.SGeomTo)))
                    return g1.SGeomTo;

                return null;
            }

            GeomWalkNfo walk(GeomWalkNfo x)
            {
                int nextElIdx = calcIdx(x.geomIdx, +1, x.lst.Count);
                var prevElIdx = calcIdx(x.geomIdx, -1, x.lst.Count);

                var nextEl = x.lst[nextElIdx];
                var prevEl = x.lst[prevElIdx];

                GeomNfo? candidate = null;
                int? candidateIdx = null;

                if (!x.geomVisited.Contains(nextEl.geom) && nextEl.inside)
                {
                    candidate = nextEl;
                    candidateIdx = nextElIdx;
                }
                else if (!x.geomVisited.Contains(prevEl.geom) && prevEl.inside)
                {
                    candidate = prevEl;
                    candidateIdx = prevElIdx;
                }

                if (candidate != null)
                {
                    var shVertex = sharedVertex(x.geom, candidate.Value.geom);
                    x.vertexVisited.Add(shVertex);
                    x.geomVisited.Add(candidate.Value.geom);
                    return new GeomWalkNfo(x.lst, x.isOnThis, candidate.Value.geom, candidateIdx.Value,
                        x.geomVisited, x.vertexVisited);
                }
                else // switch to alternate path
                {
                    Vector3D? vertex = null;
                    if (!x.vertexVisited.Contains(x.geom.SGeomFrom)) vertex = x.geom.SGeomFrom;
                    else if (!x.vertexVisited.Contains(x.geom.SGeomTo)) vertex = x.geom.SGeomTo;
                    if (vertex == null) throw new Exception($"can't find vertex to continue");
                    return getByIp(vertex, !x.isOnThis);
                }
            }

            var visitedVertexes = new HashSet<Vector3D>(ptCmp);

            foreach (var ipt in ipts)
            {
                if (visitedVertexes.Contains(ipt)) continue;

                var gLoop = new List<IEdge>();
                var gLoopHs = new HashSet<IEdge>();

                var start = getByIp(ipt, onThis: true);

                gLoop.Add(start.geom);
                gLoopHs.Add(start.geom);

                var cur = start;

                while (true)
                {
                    var next = walk(cur);

                    cur = next;

                    if (gLoopHs.Contains(cur.geom)) break;

                    gLoop.Add(cur.geom);
                    gLoopHs.Add(cur.geom);
                }

                foreach (var geom in gLoop)
                {
                    visitedVertexes.Add(geom.SGeomFrom);
                    visitedVertexes.Add(geom.SGeomTo);
                }

                yield return new Loop(tol, gLoop, checkSense: true);
            }

        }

    }

    public static partial class SciExt
    {

        public static netDxf.Entities.Hatch ToHatch(this Loop loop, double tol,
            HatchPattern pattern, bool associative = true) =>
            loop.Edges.Cast<Geometry>().ToHatch(tol, pattern, associative);

        public static LwPolyline ToLwPolyline(this Loop loop, double tol) =>
            loop.Edges.Cast<Geometry>().ToLwPolyline(tol);

        public static Plane3D DetectPlane(this IEnumerable<IEdge> edges, double tol)
        {
            var lines = new List<Line3D>();

            foreach (var edge in edges)
            {
                switch (edge.EdgeType)
                {
                    case EdgeType.Line3D: lines.Add((Line3D)edge); break;
                    case EdgeType.Circle3D: return new Plane3D(((Circle3D)edge).CS);
                    case EdgeType.Arc3D: return new Plane3D(((Arc3D)edge).CS);
                    default:
                        throw new Exception($"unexpected edge type {edge.EdgeType}");
                }
            }

            if (lines.Count == 0) throw new Exception($"can't state edges (cnt:{edges.Count()}) plane");

            return lines.BestFittingPlane(tol);
        }

        public static IEnumerable<IEdge> CheckSense(this IEnumerable<IEdge> edges, double tol)
        {
            foreach (var edgeItem in edges.WithPrev())
            {
                var edge = edgeItem.item;
                var prevEdge = edgeItem.prev;

                if (prevEdge == null) yield return edge;

                else
                {
                    if (!prevEdge.SGeomTo.EqualsTol(tol, edge.SGeomFrom))
                    {
                        if (!prevEdge.SGeomTo.EqualsTol(tol, edge.SGeomTo))
                        {
                            if (edgeItem.itemIdx == 1 && prevEdge.SGeomFrom.EqualsTol(tol, edge.SGeomFrom))
                            {
                                prevEdge.ToggleSense();
                            }
                            else
                                throw new Exception($"can't glue edge [{prevEdge}] with [{edge}]");
                        }
                        else
                            edge.ToggleSense();
                    }

                    yield return edge;
                }
            }
        }

    }

    public static partial class SciToolkit
    {

    }

}
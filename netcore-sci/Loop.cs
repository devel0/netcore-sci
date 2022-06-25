using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

using netDxf;
using netDxf.Entities;

using SearchAThing;

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

            var qits = this.Edges.Cast<Geometry>()
                .Intersect(tol, new[] { ray }, GeomSegmentMode.FromTo, GeomSegmentMode.Infinite)
                .ToList();

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

        record struct GeomNfo(IEdge geom, bool inside, bool onThis);

        class GeomNfoEqCmp : IEqualityComparer<GeomNfo>
        {
            public double Tol { get; private set; }
            public GeomNfoEqCmp(double tol)
            {
                Tol = tol;
            }

            public bool Equals(GeomNfo x, GeomNfo y) => x.geom.Equals(Tol, y.geom, includeSense: false);

            public int GetHashCode([DisallowNull] GeomNfo obj) => 0;
        }

        record struct GeomWithIdx(IEdge geom, int idx);

        record struct GeomWalkNfo(List<GeomNfo> lst, bool isOnThis, IEdge geom, int geomIdx,
            HashSet<IEdge> geomVisited, HashSet<Vector3D> vertexVisited);

        public IEnumerable<Loop> Intersect(double tol, Loop other, netDxf.DxfDocument? debugDxf = null)
        {
            var res = new List<List<IEdge>>();

            List<GeomNfo>? thisBrokenGeoms = null;
            List<GeomNfo>? otherBrokenGeoms = null;
            List<Vector3D>? ipts = null;

            netDxf.Tables.Layer? thisLayer = null;
            netDxf.Tables.Layer? otherLayer = null;
            netDxf.Tables.Layer? iptsLayer = null;
            netDxf.Tables.Layer? intersectLayer = null;

            if (debugDxf != null)
            {
                thisLayer = new netDxf.Tables.Layer("this") { Color = AciColor.Cyan };
                otherLayer = new netDxf.Tables.Layer("other") { Color = AciColor.Green };
                iptsLayer = new netDxf.Tables.Layer("ipts") { Color = AciColor.Yellow };
                intersectLayer = new netDxf.Tables.Layer("intersect") { Color = AciColor.Red };
            }

            var ptCmp = new Vector3DEqualityComparer(tol);

            {
                var thisGeoms = Edges.ToList();
                var otherGeoms = other.Edges.ToList();

                var combs = from tg in thisGeoms
                            from og in otherGeoms
                            select new { tg, og };

                var iptsNfos = combs.SelectMany(comb =>
                    ((Geometry)comb.tg).GeomIntersect(tol, (Geometry)comb.og)
                    .Where(x => x.GeomType == GeometryType.Vector3D)
                    .Select(i_nfo => new
                    {
                        ipt = (Vector3D)i_nfo,
                        tent = comb.tg,
                        oent = comb.og
                    })
                )
                .ToList()
                // filter pt duplicates                
                .GroupBy(w => w.ipt, ptCmp)
                .Select(w => w.First())
                .ToList();

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
                    .Select(w => new { ent = w.Key, breaks = w.Select(u => (Vector3D)u.ipt).ToList() })
                    .ToDictionary(k => k.ent, v => v.breaks);

                var ogeomsBreaks = iptsNfos
                    .GroupBy(w => w.oent)
                    .Select(w => new { ent = w.Key, breaks = w.Select(u => (Vector3D)u.ipt).ToList() })
                    .ToDictionary(k => k.ent, v => v.breaks);

                thisBrokenGeoms = thisGeoms.SelectMany(tgeom =>
                {
                    if (tgeomsBreaks.TryGetValue(tgeom, out var ipts))
                    {
                        var res = tgeom.Split(tol, ipts)
                            .Select(geom =>
                            {
                                var res = new GeomNfo((IEdge)geom,
                                    inside: other.ContainsPoint(tol, geom.MidPoint),
                                    onThis: true);

                                return res;
                            }).ToList();

                        return res.ToArray();
                    }
                    else
                        return new[]
                        {
                            new GeomNfo((IEdge)tgeom,
                                inside: other.ContainsPoint(tol, tgeom.MidPoint),
                                onThis: true)
                        };
                }).ToList();

                otherBrokenGeoms = otherGeoms.SelectMany(ogeom =>
                {
                    if (ogeomsBreaks.TryGetValue(ogeom, out var ipts))
                        return ogeom.Split(tol, ipts)
                            .Select(geom =>
                            {
                                var res = new GeomNfo((IEdge)geom,
                                    inside: this.ContainsPoint(tol, geom.MidPoint),
                                    onThis: false);

                                return res;
                            });
                    else
                        return new[]
                        {
                            new GeomNfo((IEdge)ogeom,
                                inside: this.ContainsPoint(tol, ogeom.MidPoint),
                                onThis: false)
                        };
                })
                .ToList();

                var overlappedCmp = new GeomNfoEqCmp(tol);
                var overlapped = otherBrokenGeoms.Intersect(thisBrokenGeoms, overlappedCmp).ToList();

                foreach (var otherOverlapped in overlapped)
                {
                    if (otherOverlapped.onThis)
                        throw new Exception($"expecting other geom insted of this for removal of intersected overlaps");
                    otherBrokenGeoms.Remove(otherOverlapped);
                }

                ipts = iptsNfos.Select(w => (Vector3D)w.ipt).ToList();
            }

            var iptsHs = ipts.ToHashSet(ptCmp);

            var ipToThisBrokenGeoms = new Dictionary<Vector3D, GeomWithIdx>(ptCmp);
            foreach (var nfo in thisBrokenGeoms.WithIndex())
            {
                var containsFrom = iptsHs.Contains(nfo.item.geom.SGeomFrom, ptCmp);
                var containsTo = iptsHs.Contains(nfo.item.geom.SGeomTo, ptCmp);
                if (nfo.item.inside)
                {
                    if (containsFrom)
                    {
                        if (!ipToThisBrokenGeoms.ContainsKey(nfo.item.geom.SGeomFrom))
                            ipToThisBrokenGeoms.Add(nfo.item.geom.SGeomFrom, new GeomWithIdx(nfo.item.geom, nfo.idx));
                    }

                    if (containsTo)
                    {
                        if (!ipToThisBrokenGeoms.ContainsKey(nfo.item.geom.SGeomTo))
                            ipToThisBrokenGeoms.Add(nfo.item.geom.SGeomTo, new GeomWithIdx(nfo.item.geom, nfo.idx));
                    }
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
                    {
                        if (!ipToOtherBrokenGeoms.ContainsKey(nfo.item.geom.SGeomFrom))
                            ipToOtherBrokenGeoms.Add(nfo.item.geom.SGeomFrom, new GeomWithIdx(nfo.item.geom, nfo.idx));
                    }

                    if (containsTo)
                    {
                        if (!ipToOtherBrokenGeoms.ContainsKey(nfo.item.geom.SGeomTo))
                            ipToOtherBrokenGeoms.Add(nfo.item.geom.SGeomTo, new GeomWithIdx(nfo.item.geom, nfo.idx));
                    }
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
                if (g1.SGeomFrom.EqualsTol(tol, g2.SGeomFrom) || (g1.SGeomFrom.EqualsTol(tol, g2.SGeomTo)))
                    return g1.SGeomFrom;

                if (g1.SGeomTo.EqualsTol(tol, g2.SGeomFrom) || (g1.SGeomTo.EqualsTol(tol, g2.SGeomTo)))
                    return g1.SGeomTo;

                return null;
            }

            GeomWalkNfo? walk(GeomWalkNfo x)
            {
                int nextElIdx = calcIdx(x.geomIdx, +1, x.lst.Count);
                var prevElIdx = calcIdx(x.geomIdx, -1, x.lst.Count);

                var nextEl = x.lst[nextElIdx];
                var prevEl = x.lst[prevElIdx];

                GeomNfo? candidate = null;
                int? candidateIdx = null;
                Vector3D? candidateShVertex = null;

                if (!x.geomVisited.Contains(nextEl.geom) && nextEl.inside &&
                    (candidateShVertex = sharedVertex(x.geom, nextEl.geom)) != null)
                {
                    candidate = nextEl;
                    candidateIdx = nextElIdx;
                }
                else if (!x.geomVisited.Contains(prevEl.geom) && prevEl.inside &&
                    (candidateShVertex = sharedVertex(x.geom, prevEl.geom)) != null)
                {
                    candidate = prevEl;
                    candidateIdx = prevElIdx;
                }

                if (candidate != null && candidateIdx != null && candidateShVertex != null)
                {
                    x.vertexVisited.Add(candidateShVertex);
                    x.geomVisited.Add(candidate.Value.geom);
                    return new GeomWalkNfo(x.lst, x.isOnThis,
                        candidate.Value.geom, candidateIdx.Value,
                        x.geomVisited, x.vertexVisited);
                }
                else // switch to alternate path
                {
                    Vector3D? vertex = null;
                    if (!x.vertexVisited.Contains(x.geom.SGeomFrom)) vertex = x.geom.SGeomFrom;
                    else if (!x.vertexVisited.Contains(x.geom.SGeomTo)) vertex = x.geom.SGeomTo;

                    if (vertex != null)
                        return getByIp(vertex, !x.isOnThis);
                    else
                        return null;
                }
            }

            if (debugDxf != null)
            {
                foreach (var x in thisBrokenGeoms.WithIndex())
                {
                    var ent = x.item.geom.DxfEntity;
                    ent.Layer = thisLayer;
                    debugDxf.AddEntity(ent);

                    debugDxf.AddEntity(new netDxf.Entities.Text(x.idx.ToString(),
                        ((Geometry)x.item.geom).MidPoint.ToDxfVector2(), .7)
                        .Eval(e =>
                        {
                            e.Alignment = TextAlignment.MiddleLeft;
                            e.Layer = thisLayer;
                            return e;
                        }));
                }

                foreach (var x in otherBrokenGeoms.WithIndex())
                {
                    var ent = x.item.geom.DxfEntity;
                    ent.Layer = otherLayer;
                    debugDxf.AddEntity(ent);

                    debugDxf.AddEntity(new netDxf.Entities.Text(x.idx.ToString(),
                        ((Geometry)x.item.geom).MidPoint.ToDxfVector2(), .7)
                        .Eval(e =>
                        {
                            e.Alignment = TextAlignment.MiddleLeft;
                            e.Layer = otherLayer;
                            return e;
                        }));
                }

                foreach (var ip in ipts)
                {
                    var ent = ip.DxfEntity;
                    ent.Layer = iptsLayer;
                    debugDxf.AddEntity(ent);
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
                    if (gLoop.Count == 2)
                    {
                        start.vertexVisited.Add(start.geom.SGeomFrom);
                        start.vertexVisited.Add(start.geom.SGeomTo);
                    }
                    // if (gLoop.Count > 2 && sharedVertex(cur.geom, gLoop[0]) != null)
                    //      break;

                    var qnext = walk(cur);

                    if (qnext == null) break;

                    cur = qnext.Value; // BREAKPOINT

                    if (gLoopHs.Contains(cur.geom)) break;
                    if (
                        visitedVertexes.Contains(cur.geom.SGeomFrom, ptCmp) &&
                        visitedVertexes.Contains(cur.geom.SGeomTo, ptCmp))
                        break;

                    gLoop.Add(cur.geom);
                    gLoopHs.Add(cur.geom);

                    visitedVertexes.Add(cur.geom.SGeomFrom);
                    visitedVertexes.Add(cur.geom.SGeomTo);
                }

                // foreach (var geom in gLoop)
                // {
                //     visitedVertexes.Add(geom.SGeomFrom);
                //     visitedVertexes.Add(geom.SGeomTo);
                // }

                var loopres = new Loop(tol, gLoop, checkSense: true);

                if (debugDxf != null)
                {
                    var lw = loopres.ToLwPolyline(tol);
                    lw.Layer = intersectLayer;
                    debugDxf.AddEntity(lw);
                }

                yield return loopres;
            }

        }

        /// <summary>
        /// create an hatch from given loop, pattern
        /// </summary>        
        public netDxf.Entities.Hatch ToHatch(double tol, HatchPattern pattern, bool associative = true) =>
            Edges.Cast<Geometry>().ToHatch(tol, pattern, associative);

        public LwPolyline ToLwPolyline(double tol) => Edges.Cast<Geometry>().ToLwPolyline(tol);

        public override string ToString()
        {
            return $"Edges:{Edges.Count}";
        }

    }

    public static partial class SciExt
    {

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

        /// <summary>
        /// from given set of edges returns the same set eventually toggling sense of edges to make them glue so that SGeomTo of previous equals SGeomFrom of current.
        /// it can raise exception if there isn't availability to glue edges regardless toggling their sense.
        /// first element will toggled to match second one, then other elements will follow the sense matching established.
        /// </summary>        
        public static IEnumerable<IEdge> CheckSense(this IEnumerable<IEdge> edges, double tol)
        {
            IEdge? overrideCur = null;

            var lst = edges.ToList();

            foreach (var edgeItem in edges.WithNext())
            {
                if (edgeItem.next == null)
                {
                    if (overrideCur != null)
                        yield return overrideCur;
                    else
                        yield return edgeItem.item;
                }

                else
                {
                    var cur = edgeItem.item;
                    
                    if (overrideCur != null) cur = overrideCur;

                    var q = cur.CheckSense(tol, edgeItem.next);

                    if (q == null || (overrideCur != null && q.Value.needToggleSenseThis))
                        throw new Exception($"can't glue [{cur}] with [{edgeItem.next}]");

                    overrideCur = null;

                    if (q.Value.needToggleSenseThis)
                        yield return (IEdge)cur.ToggleSense();
                    else
                        yield return cur;

                    if (q.Value.needToggleSenseNext)
                        overrideCur = (IEdge)edgeItem.next.ToggleSense();
                }
            }
        }

    }

    public static partial class SciToolkit
    {

    }

}
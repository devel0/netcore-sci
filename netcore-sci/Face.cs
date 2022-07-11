using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

using netDxf;
using netDxf.Entities;

using SearchAThing;
using System.Text;
using Newtonsoft.Json;
using netDxf.Tables;

namespace SearchAThing
{

    public class Face
    {
        public Plane3D Plane { get; private set; }

        public IReadOnlyList<Loop> Loops { get; private set; }

        /// <summary>
        /// planar face with outer and optional inner loops
        /// </summary>
        /// <param name="plane">plane where loop lies</param>
        /// <param name="loops">loops ( first is the outer )</param>
        public Face(Plane3D plane, IReadOnlyList<Loop> loops)
        {
            Plane = plane;
            Loops = loops;
        }

        /// <summary>
        /// build a face with given outer loop
        /// </summary>
        public Face(Loop loop)
        {
            Plane = loop.Plane;
            Loops = new[] { loop };
        }

        /// <summary>
        /// return new face translated of given delta
        /// </summary>
        public Face Move(Vector3D delta) => new Face(Plane.Move(delta), Loops.Select(l => l.Move(delta)).ToList());

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

        record struct GeomWithIdx(GeomNfo nfo, int idx);

        record struct GeomWalkNfo(List<GeomNfo> lst, bool isOnThis, IEdge geom, int geomIdx,
            HashSet<IEdge> geomVisited, HashSet<Vector3D> vertexVisited);

        public enum BooleanMode
        {
            Intersect,

            Union,

            Difference
        };

        /// <summary>
        /// boolean operation with this and other loops.
        /// precondition: must coplanar
        /// </summary>        
        public IEnumerable<Face> Boolean(double tol, Face other,
            BooleanMode mode = BooleanMode.Intersect,
            netDxf.DxfDocument? debugDxf = null)
        {
            if (mode == BooleanMode.Union) throw new NotImplementedException();

            var res = new List<List<IEdge>>();

            List<GeomNfo>? thisBrokenGeoms = null;
            List<GeomNfo>? otherBrokenGeoms = null;
            List<Vector3D>? ipts = null;

            netDxf.Tables.Layer? thisOrigLayer = null;
            netDxf.Tables.Layer? otherOrigLayer = null;
            netDxf.Tables.Layer? thisLayer = null;
            netDxf.Tables.Layer? otherLayer = null;
            netDxf.Tables.Layer? iptsLayer = null;
            netDxf.Tables.Layer? booleanLayer = null;

            if (debugDxf != null)
            {
                thisOrigLayer = new netDxf.Tables.Layer("orig_this") { Color = AciColor.Yellow };
                otherOrigLayer = new netDxf.Tables.Layer("orig_other") { Color = AciColor.Green };
                thisLayer = new netDxf.Tables.Layer("this") { Color = AciColor.Yellow };
                otherLayer = new netDxf.Tables.Layer("other") { Color = AciColor.Green };
                iptsLayer = new netDxf.Tables.Layer("ipts") { Color = AciColor.Cyan };
                booleanLayer = new netDxf.Tables.Layer("boolean") { Color = AciColor.Red };
            }

            var ptCmp = new Vector3DEqualityComparer(tol);
            {
                var thisGeoms = this.Loops[0].Edges.ToList();
                var otherGeoms = other.Loops[0].Edges.ToList();

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
                        oent = comb.og,

                        // ipt_strictly_inside_this = this.ContainsPoint(tol, (Vector3D)i_nfo, excludePerimeter: true),
                        // ipt_strictly_inside_other = other.ContainsPoint(tol, (Vector3D)i_nfo, excludePerimeter: true)
                    })
                )
                .ToList()
                // filter pt duplicates                
                .GroupBy(w => w.ipt, ptCmp)
                .Select(w => w.First())
                .ToList();

                // this contains other, viceversa and disjoint
                if (iptsNfos.Count == 0)
                {
                    // this contains other
                    var opts = other.Loops[0].Edges.Select(edge => (Vector3D)edge.SGeomFrom).ToList();
                    if (opts.All(opt => this.Loops[0].ContainsPoint(tol, opt)))
                        yield return other;

                    // other contains this
                    var tpts = this.Loops[0].Edges.Select(edge => (Vector3D)edge.SGeomFrom).ToList();
                    if (tpts.All(opt => other.Loops[0].ContainsPoint(tol, opt)))
                        yield return this;

                    // disjoint
                    switch (mode)
                    {
                        case BooleanMode.Union: yield return this; yield return other; yield break;
                        case BooleanMode.Difference: yield return this; break;
                    }
                    yield break;
                }

                // this share only edges with other but not intersects
                // if (iptsNfos.All(nfo => !nfo.ipt_strictly_inside_this && !nfo.ipt_strictly_inside_other)) yield break;

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
                                    inside: other.Loops[0].ContainsPoint(tol, geom.MidPoint),
                                    onThis: true);

                                return res;
                            }).ToList();

                        return res.ToArray();
                    }
                    else
                        return new[]
                        {
                            new GeomNfo((IEdge)tgeom,
                                inside: other.Loops[0].ContainsPoint(tol, tgeom.MidPoint),
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
                                    inside: this.Loops[0].ContainsPoint(tol, geom.MidPoint),
                                    onThis: false);

                                return res;
                            });
                    else
                        return new[]
                        {
                            new GeomNfo((IEdge)ogeom,
                                inside: this.Loops[0].ContainsPoint(tol, ogeom.MidPoint),
                                onThis: false)
                        };
                })
                .ToList();

                var overlappedCmp = new GeomNfoEqCmp(tol);
                var overlaps = new List<(GeomNfo toverlap, GeomNfo ooverlap)>();
                foreach (var tbg in thisBrokenGeoms)
                {
                    foreach (var obg in otherBrokenGeoms)
                    {
                        if (overlappedCmp.Equals(tbg, obg)) overlaps.Add((tbg, obg));
                    }
                }

                foreach (var overlap in overlaps)
                {
                    otherBrokenGeoms.Remove(overlap.ooverlap);
                    var x = overlap.toverlap;
                    x.inside = true;
                }

                ipts = iptsNfos.Select(w => (Vector3D)w.ipt).ToList();
            }

            var iptsHs = ipts.ToHashSet(ptCmp);

            var ipToBrokenGeoms = new Dictionary<Vector3D, List<GeomWithIdx>>(ptCmp);

            void ScanBrokenGeoms(List<GeomNfo> brokenGeoms)
            {
                foreach (var nfo in brokenGeoms.WithIndex())
                {
                    List<GeomWithIdx>? lst;

                    var containsFrom = iptsHs.Contains(nfo.item.geom.SGeomFrom, ptCmp);
                    var containsTo = iptsHs.Contains(nfo.item.geom.SGeomTo, ptCmp);

                    if (containsFrom)
                    {
                        if (!ipToBrokenGeoms.TryGetValue(nfo.item.geom.SGeomFrom, out lst))
                        {
                            lst = new List<GeomWithIdx>();
                            ipToBrokenGeoms.Add(nfo.item.geom.SGeomFrom, lst);
                        }
                        lst.Add(new GeomWithIdx(nfo.item, nfo.idx));
                    }

                    if (containsTo)
                    {
                        if (!ipToBrokenGeoms.TryGetValue(nfo.item.geom.SGeomTo, out lst))
                        {
                            lst = new List<GeomWithIdx>();
                            ipToBrokenGeoms.Add(nfo.item.geom.SGeomTo, lst);
                        }
                        lst.Add(new GeomWithIdx(nfo.item, nfo.idx));
                    }
                }
            }

            ScanBrokenGeoms(thisBrokenGeoms);
            ScanBrokenGeoms(otherBrokenGeoms);

            if (debugDxf != null)
            {
                foreach (var ipt in ipts)
                {
                    var cond = false;

                    switch (mode)
                    {

                        case BooleanMode.Intersect: cond = true; break;

                        case BooleanMode.Difference:
                            {
                                var q = ipToBrokenGeoms[ipt];

                                var inside_cnt = 0;
                                var outside_cnt = 0;
                                foreach (var x in q)
                                {
                                    if (x.nfo.inside) ++inside_cnt;
                                    else ++outside_cnt;
                                }

                                cond = inside_cnt > 0 && outside_cnt > 0;
                            }
                            break;

                    }

                    if (cond)
                    {
                        var ent = ipt.DxfEntity;
                        ent.Layer = iptsLayer;
                        debugDxf.AddEntity(ent);
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

            Vector3D? sharedVertex(IEdge g1, IEdge g2)
            {
                if (g1.SGeomFrom.EqualsTol(tol, g2.SGeomFrom) || (g1.SGeomFrom.EqualsTol(tol, g2.SGeomTo)))
                    return g1.SGeomFrom;

                if (g1.SGeomTo.EqualsTol(tol, g2.SGeomFrom) || (g1.SGeomTo.EqualsTol(tol, g2.SGeomTo)))
                    return g1.SGeomTo;

                return null;
            }

            GeomWalkNfo? getByIp(Vector3D ip, bool onThis, bool searchInside = true)
            {
                var qt = ipToBrokenGeoms[ip];
                var q = qt.FirstOrDefault(w => w.nfo.inside == searchInside && onThis == w.nfo.onThis);

                if (q == null) return null;

                var lst = onThis ? thisBrokenGeoms : otherBrokenGeoms;

                var geomVisited = onThis ? thisGeomVisited : otherGeomVisited;
                var vertexVisited = onThis ? thisVertexVisited : otherVertexVisited;

                geomVisited.Add(q.nfo.geom);
                vertexVisited.Add(ip);

                return new GeomWalkNfo(lst, isOnThis: onThis, q.nfo.geom, q.idx, geomVisited, vertexVisited);
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
                var candidateInsideness = mode == BooleanMode.Intersect || !x.isOnThis;

                if (!x.geomVisited.Contains(nextEl.geom) &&
                    nextEl.inside == candidateInsideness &&
                    (candidateShVertex = sharedVertex(x.geom, nextEl.geom)) != null &&
                    (
                        (mode != BooleanMode.Difference)
                        ||
                        (candidateShVertex.EqualsTol(tol, x.geom.SGeomFrom) || candidateShVertex.EqualsTol(tol, x.geom.SGeomTo))
                    ))
                {
                    candidate = nextEl;
                    candidateIdx = nextElIdx;
                }
                else if (!x.geomVisited.Contains(prevEl.geom) &&
                    prevEl.inside == candidateInsideness &&
                    (candidateShVertex = sharedVertex(x.geom, prevEl.geom)) != null &&
                    (
                        (mode != BooleanMode.Difference)
                        ||
                        (candidateShVertex.EqualsTol(tol, x.geom.SGeomFrom) || candidateShVertex.EqualsTol(tol, x.geom.SGeomTo))
                    ))
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
                    if (x.geom == null) return null;

                    if (!x.vertexVisited.Contains(x.geom.SGeomFrom)) vertex = x.geom.SGeomFrom;

                    else if (!x.vertexVisited.Contains(x.geom.SGeomTo)) vertex = x.geom.SGeomTo;

                    if (vertex != null)
                    {
                        List<GeomWithIdx>? lst;
                        if (ipToBrokenGeoms.TryGetValue(vertex, out lst))
                        {
                            if (x.isOnThis && lst.Any(w => w.nfo.inside && !w.nfo.onThis))
                                return getByIp(vertex, onThis: false);

                            if (!x.isOnThis
                                &&
                                (
                                    (mode == BooleanMode.Intersect && lst.Any(w => w.nfo.inside && !w.nfo.onThis))
                                    ||
                                    (mode == BooleanMode.Difference && lst.Any(w => !w.nfo.inside && w.nfo.onThis))
                                ))
                                return getByIp(vertex, onThis: true,
                                    searchInside: mode == BooleanMode.Intersect);
                        }

                        return null;
                    }
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
                            e.Alignment = TextAlignment.Middle;
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
                            e.Alignment = TextAlignment.Middle;
                            e.Layer = otherLayer;
                            return e;
                        }));
                }
            }

            var visitedVertexes = new HashSet<Vector3D>(ptCmp);

            if (ipts.Count == 1) yield break;

            foreach (var ipt in ipts)
            {
                if (visitedVertexes.Contains(ipt)) continue;

                var gLoop = new List<IEdge>();
                var gLoopHs = new HashSet<IEdge>();

                var qstart = getByIp(ipt, onThis: true, searchInside: mode != BooleanMode.Difference);

                //if (qstart == null || qstart.Value.geom == null) break;
                if (qstart == null || qstart.Value.geom == null)
                {
                    visitedVertexes.Add(ipt);
                    continue;
                }

                var start = qstart.Value;

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

                    var qnext = walk(cur);

                    if (qnext == null) break;

                    cur = qnext.Value;

                    if (gLoopHs.Contains(cur.geom)) break;

                    if (cur.geom == null) break;

                    if (
                        visitedVertexes.Contains(cur.geom.SGeomFrom, ptCmp) &&
                        visitedVertexes.Contains(cur.geom.SGeomTo, ptCmp))
                        break;

                    gLoop.Add(cur.geom);
                    gLoopHs.Add(cur.geom);

                    visitedVertexes.Add(cur.geom.SGeomFrom);
                    visitedVertexes.Add(cur.geom.SGeomTo);
                }

                if (gLoop.Count == 1 || (gLoop.Count == 2 && gLoop.All(w => w.EdgeType == EdgeType.Line3D))) yield break;

                var loopres = new Loop(tol, this.Plane, gLoop, checkSense: true);

                if (debugDxf != null)
                {
                    var lw = loopres.ToLwPolyline(tol);
                    lw.Layer = booleanLayer;
                    debugDxf.AddEntity(lw);
                }

                yield return loopres.ToFace();
            }

        }

        public IEnumerable<LwPolyline> DxfEntities(double tol) => Loops.Select(w => w.DxfEntity(tol));

    }

    public static partial class SciExt
    {

        /// <summary>
        /// build planar face with given loops ( first is the outer );
        /// precondition: loops must lie on same plane
        /// </summary>
        public static Face ToFace(this IEnumerable<netDxf.Entities.LwPolyline> lwpolyline, double tol)
        {
            var loops = lwpolyline.Select(w => w.ToLoop(tol)).ToList();

            if (loops.Count > 1)
            {
                loops = loops.Select(loop => new
                {
                    loop,
                    area = loop.Area
                })
                .ToList()
                .OrderByDescending(w => w.area)
                .Select(w => w.loop)
                .ToList();
            }

            return new Face(loops[0].Plane, loops);
        }

        /// <summary>
        /// build face with given outer loop
        /// </summary>        
        public static Face ToFace(this Loop loop) => new Face(loop);

    }

}
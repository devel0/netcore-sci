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

        class LoopNfo
        {
            public Face faceOwner { get; private set; }

            public bool onThis { get; private set; }

            public Loop loop { get; private set; }

            public bool outer { get; private set; }

            /// <summary>
            /// hashset of loopnfo that contains or with geom equals this one ( all levels )
            /// </summary>            
            internal HashSet<LoopNfo> parentLoopNfos = new HashSet<LoopNfo>();

            /// <summary>
            /// hashset of loopnfo contained or with geom equals this one ( all levels )
            /// </summary>            
            internal HashSet<LoopNfo> childrenLoopNfos = new HashSet<LoopNfo>();

            /// <summary>
            /// is the first parent loop that has an Area equals or greather than this ( first direct level )
            /// </summary>
            internal LoopNfo? directParent = null;

            /// <summary>
            /// hashset of loopnfo contained or with geom equals this one ( first direct level )
            /// </summary>            
            internal HashSet<LoopNfo> directChildrenLoopNfos = new HashSet<LoopNfo>();

            public LoopNfo(Face faceOwner, bool onThis, Loop loop, bool outer)
            {
                this.faceOwner = faceOwner;
                this.onThis = onThis;
                this.loop = loop;
                this.outer = outer;
            }

            public override string ToString() => $"A:{loop.Area} onThis:{onThis} outer:{outer} edges:{loop.Edges.Count} parentLoops:{parentLoopNfos.Count}";
        }

        class EdgeNfo
        {

            public LoopNfo loopOwner { get; private set; }

            public IEdge edge { get; private set; }

            public bool onThis { get; private set; }

            public EdgeNfo(LoopNfo loopOwner, IEdge edge, bool onThis)
            {
                this.loopOwner = loopOwner;
                this.edge = edge;
                this.onThis = onThis;
            }

            public override string ToString() => $"onThis:{onThis} loopOuter:{loopOwner.outer} edge:{edge}";
        }

        public enum BooleanMode
        {
            Union,

            Intersect,

            Difference,
        };

        /// <summary>
        /// boolean operation with this and other loops.
        /// precondition: must coplanar
        /// </summary>        
        public IEnumerable<Face> Boolean(double tol, Face other,
            BooleanMode mode = BooleanMode.Intersect,
            netDxf.DxfDocument? debugDxf = null)
        {
            var ptCmp = new Vector3DEqualityComparer(tol);

            var res = new List<List<IEdge>>();

            List<EdgeNfo>? thisLoopBrokenEdgeNfos = null;
            List<EdgeNfo>? otherLoopBrokenEdgeNfos = null;
            var ips = new HashSet<Vector3D>(ptCmp);
            var edgeVertexes = new HashSet<Vector3D>(ptCmp);
            var vertexToEdgeNfos = new Dictionary<Vector3D, List<EdgeNfo>>(ptCmp);

            var thisLoopNfos = this.Loops.Select((loop, idx) =>
                new LoopNfo(faceOwner: this, onThis: true, loop, outer: idx == 0)).ToList();

            var otherLoopNfos = other.Loops.Select((loop, idx) =>
                new LoopNfo(faceOwner: other, onThis: false, loop, outer: idx == 0)).ToList();

            void UpdateLoopNfo(LoopNfo loopNfo1, LoopNfo loopNfo2)
            {
                if (loopNfo1.loop.Contains(tol, loopNfo2.loop, excludePerimeter: false))
                {
                    loopNfo2.parentLoopNfos.Add(loopNfo1);
                    loopNfo1.childrenLoopNfos.Add(loopNfo2);
                }

                else if (loopNfo2.loop.Contains(tol, loopNfo1.loop, excludePerimeter: false))
                {
                    loopNfo1.parentLoopNfos.Add(loopNfo2);
                    loopNfo2.childrenLoopNfos.Add(loopNfo1);
                }
            }

            {
                var allLoopNfos = thisLoopNfos.Union(otherLoopNfos).ToList();

                //
                // update {thisLoopNfos, otherLoopNfos}.parentLoopNfos
                //
                {
                    foreach (var comb in (
                        from loop1 in allLoopNfos
                        from loop2 in allLoopNfos
                        where loop1 != loop2
                        select new { loop1, loop2 }))
                        UpdateLoopNfo(comb.loop1, comb.loop2);

                    foreach (var loopNfo in allLoopNfos)
                        loopNfo.directParent = loopNfo.parentLoopNfos.OrderBy(w => w.loop.Area).FirstOrDefault();

                    foreach (var loopNfo in allLoopNfos)
                    {
                        foreach (var x in loopNfo.childrenLoopNfos.Where(r => r.directParent == loopNfo))
                            loopNfo.directChildrenLoopNfos.Add(x);
                    }
                }

                var thisLoopEdgeNfos = thisLoopNfos.SelectMany(loopNfo =>
                    loopNfo.loop.Edges.Select(edge => new EdgeNfo(loopOwner: loopNfo, edge, onThis: true))).ToList();

                var otherLoopEdgeNfos = otherLoopNfos.SelectMany(loopNfo =>
                    loopNfo.loop.Edges.Select(edge => new EdgeNfo(loopOwner: loopNfo, edge, onThis: false))).ToList();

                var combs = from thisEdgeNfo in thisLoopEdgeNfos
                            from otherEdgeNfo in otherLoopEdgeNfos
                            select new { thisEdgeNfo, otherEdgeNfo };

                var ipNfos = combs.SelectMany(comb =>
                    ((Geometry)comb.thisEdgeNfo.edge)
                    .GeomIntersect(tol, (Geometry)comb.otherEdgeNfo.edge)
                    .Where(x => x.GeomType == GeometryType.Vector3D)
                    .Select(i_nfo => new
                    {
                        ip = (Vector3D)i_nfo,

                        thisEdgeNfo = comb.thisEdgeNfo,
                        otherEdgeNfo = comb.otherEdgeNfo,
                    }))
                    .ToList();

                var thisOuterLoopNfo = thisLoopNfos[0];
                var otherOuterLoopNfo = otherLoopNfos[0];

                //
                // special case : no ips
                //
                if (ipNfos.Count == 0)
                {
                    // this and other totally disjoint
                    if (thisOuterLoopNfo.parentLoopNfos.Count == 0 && otherOuterLoopNfo.parentLoopNfos.Count == 0)
                    {
                        switch (mode)
                        {
                            case BooleanMode.Union:
                                yield return this;
                                yield return other;
                                break;

                            case BooleanMode.Intersect:
                                yield break;

                            case BooleanMode.Difference:
                                yield return this;
                                break;
                        }
                    }
                    else
                        switch (mode)
                        {
                            case BooleanMode.Union:
                                {
                                    var externalOuter = allLoopNfos.First(loopNfo => loopNfo.directParent == null);

                                    if (externalOuter.childrenLoopNfos.All(loopNfo => loopNfo.faceOwner != externalOuter.faceOwner))
                                        yield return externalOuter.faceOwner;

                                    else
                                    {
                                        if (externalOuter.directChildrenLoopNfos.All(loopNfo => loopNfo.faceOwner == externalOuter.faceOwner))
                                        {
                                            yield return externalOuter.faceOwner;

                                            yield return allLoopNfos.First(loopNfo => loopNfo.faceOwner != externalOuter.faceOwner).faceOwner;
                                        }
                                        else
                                        {
                                            var effectiveInnerLoops = allLoopNfos.Where(loopNfo =>
                                                !loopNfo.outer && !loopNfo.directParent!.outer)
                                                .Select(loopNfo => loopNfo.loop).ToList();

                                            if (effectiveInnerLoops.Count == 0)
                                            {
                                                if (externalOuter.faceOwner.Loops.Count == 1)
                                                    yield return externalOuter.faceOwner;
                                                else
                                                    yield return new Face(Plane, new[] { externalOuter.loop });
                                            }

                                            else yield return new Face(Plane,
                                                new[] { externalOuter.loop }.Union(effectiveInnerLoops).ToArray());
                                        }
                                    }
                                }
                                break;

                            case BooleanMode.Intersect:
                                {
                                    var enclosedOuter = allLoopNfos.First(loopNfo => loopNfo.outer && loopNfo.directParent != null);

                                    if (enclosedOuter != null && enclosedOuter.directParent!.outer)
                                    {
                                        yield return new Face(Plane,
                                            new[] { enclosedOuter.loop }.Union(enclosedOuter.directChildrenLoopNfos.Select(w => w.loop)).ToArray());
                                    }
                                }
                                break;

                            case BooleanMode.Difference:
                                {
                                    var enclosedOuter = allLoopNfos.First(loopNfo => loopNfo.outer && loopNfo.directParent != null);

                                    if (enclosedOuter.directParent!.onThis)
                                    {
                                        if (enclosedOuter.directParent.outer)
                                        {
                                            yield return new Face(Plane, new[] { thisOuterLoopNfo.loop, otherOuterLoopNfo.loop });

                                            foreach (var face in enclosedOuter.directChildrenLoopNfos.Where(w => !w.onThis)
                                                .Select(loopNfo => new Face(Plane, new[] { loopNfo.loop }.Union(loopNfo.childrenLoopNfos.Select(w => w.loop)).ToArray())))
                                                yield return face;
                                        }
                                        else
                                            yield return thisOuterLoopNfo.faceOwner;
                                    }
                                    else // enclosedOuter.onThis
                                    {
                                        if (!enclosedOuter.directParent.outer)
                                            yield return thisOuterLoopNfo.faceOwner;
                                        else
                                        {
                                            var enclosedOuterOtherChild = enclosedOuter.childrenLoopNfos
                                                .FirstOrDefault(loopNfo => !loopNfo.onThis);

                                            if (enclosedOuterOtherChild != null &&
                                                enclosedOuterOtherChild.directParent == enclosedOuter)
                                            {
                                                var loops = new List<Loop> { enclosedOuterOtherChild.loop };
                                                loops.AddRange(enclosedOuterOtherChild.childrenLoopNfos
                                                    //.Where(loopNfo=> !loopNfo.directParent!.outer)
                                                    .Select(w => w.loop));

                                                yield return new Face(Plane, loops);
                                            }
                                        }

                                    }

                                }
                                break;
                        }

                    yield break;
                }

                var thisLoopEdgeNfoToBreaks = ipNfos
                    .GroupBy(w => w.thisEdgeNfo)
                    .Select(w => new { edgeNfo = w.Key, edgeBreakIps = w.Select(u => (Vector3D)u.ip).ToList() })
                    .ToDictionary(k => k.edgeNfo, v => v.edgeBreakIps);

                var otherLoopEdgeNfoToBreaks = ipNfos
                    .GroupBy(w => w.otherEdgeNfo)
                    .Select(w => new { edgeNfo = w.Key, edgeBreakIps = w.Select(u => (Vector3D)u.ip).ToList() })
                    .ToDictionary(k => k.edgeNfo, v => v.edgeBreakIps);

                List<EdgeNfo> BuildBrokenNfos(
                    List<EdgeNfo> edgeNfos,
                    Dictionary<EdgeNfo, List<Vector3D>> loopEdgeToBreaks) => edgeNfos.SelectMany(edgeNfo =>
                    {
                        if (loopEdgeToBreaks.TryGetValue(edgeNfo, out var breaks))
                        {
                            return edgeNfo.edge.Split(tol, breaks)
                                .Select(geom => new EdgeNfo(edgeNfo.loopOwner, (IEdge)geom, edgeNfo.onThis)).ToArray();
                        }
                        else
                            return new[] { edgeNfo };
                    }).ToList();

                thisLoopBrokenEdgeNfos = BuildBrokenNfos(thisLoopEdgeNfos, thisLoopEdgeNfoToBreaks);
                otherLoopBrokenEdgeNfos = BuildBrokenNfos(otherLoopEdgeNfos, otherLoopEdgeNfoToBreaks);

                foreach (var ipNfo in ipNfos) ips.Add(ipNfo.ip);

                foreach (var edgeNfo in thisLoopBrokenEdgeNfos.Union(otherLoopBrokenEdgeNfos))
                {
                    void recNfo(Vector3D vertex)
                    {
                        edgeVertexes.Add(vertex);

                        if (!vertexToEdgeNfos.TryGetValue(vertex, out var edgeNfos))
                        {
                            edgeNfos = new List<EdgeNfo> { edgeNfo };
                            vertexToEdgeNfos.Add(vertex, edgeNfos);
                        }
                        else
                            edgeNfos.Add(edgeNfo);
                    }

                    recNfo(edgeNfo.edge.SGeomFrom);
                    recNfo(edgeNfo.edge.SGeomTo);
                }
            }

            switch (mode)
            {
                case BooleanMode.Intersect:
                    {

                    }
                    break;
            }

            yield break;
        }

        public IEnumerable<LwPolyline> DxfEntities(double tol) => Loops.Select(w => w.DxfEntity(tol));

        /// <summary>
        /// create hatch with outer and inner boundaries
        /// </summary>        
        public netDxf.Entities.Hatch? ToHatch(double tol, HatchPattern pattern, bool associative = true)
        {
            if (Loops.Count == 0) return null;

            var loopLws = Loops.Select(loop => loop.ToLwPolyline(tol)).ToList();

            var hatch = new netDxf.Entities.Hatch(pattern,
                loopLws.Select(loopLw => new HatchBoundaryPath(new[] { loopLw })),
                associative);
            hatch.Normal = loopLws[0].Normal;
            hatch.Elevation = loopLws[0].Elevation;
            return hatch;
        }

    }

    public static partial class SciExt
    {

        /// <summary>
        /// build planar face with given loops ( first is the outer );
        /// input loop can be unordered ( loop with greather area will be considered as outer loop );
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
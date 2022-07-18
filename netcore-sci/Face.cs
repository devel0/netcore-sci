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
using System.Diagnostics;

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

        double? _Area = null;

        /// <summary>
        /// (cached) area of face ( outer loop area - inner loops area )
        /// </summary>
        /// <value></value>
        public double Area
        {
            get
            {
                if (_Area == null)
                    _Area = Loops[0].Area - Loops.Skip(1).Select(w => w.Area).Sum();

                return _Area.Value;
            }
        }

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

            bool? _occluded = null;
            /// <summary>
            /// (updated after broken geom)
            /// states if edge not totally on counterpart perimeter but something is inside
            /// </summary>            
            public bool occluded
            {
                get
                {
                    if (_occluded == null) throw new Exception($"occluded not setup on this object");
                    return _occluded.Value;
                }
                internal set
                {
                    _occluded = value;
                }
            }

            bool? _overlapped = null;
            /// <summary>
            /// (updated after broken geom)
            /// states if this edge overlapped with counterpart
            /// </summary>            
            public bool overlapped
            {
                get
                {
                    if (_overlapped == null) throw new Exception($"overlapped not setup on this object");
                    return _overlapped.Value;
                }
                internal set
                {
                    _overlapped = value;
                }
            }

            public EdgeNfo(LoopNfo loopOwner, IEdge edge, bool onThis)
            {
                this.loopOwner = loopOwner;
                this.edge = edge;
                this.onThis = onThis;
            }

            public override string ToString() => $"{(onThis ? "" : "!")}onThis {(occluded == true ? "" : "!")}occluded {(overlapped == true ? "" : "!")}overlapped {(loopOwner.outer ? "" : "!")}loopOuter edge:{edge}";
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
                if (loopNfo1.loop.Contains(tol, loopNfo2.loop))
                {
                    loopNfo2.parentLoopNfos.Add(loopNfo1);
                    loopNfo1.childrenLoopNfos.Add(loopNfo2);
                }

                else if (loopNfo2.loop.Contains(tol, loopNfo1.loop))
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

                #region special case : no ips

                if (ipNfos.Count == 0)
                {
                    // this and other totally disjoint
                    if (thisOuterLoopNfo.parentLoopNfos.Count == 0 && otherOuterLoopNfo.parentLoopNfos.Count == 0)
                    {
                        switch (mode)
                        {
                            case BooleanMode.Union:
                                var q = this.Loops[0].Contains(tol, other.Loops[0]);
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

                #endregion

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

                void updateVertexToEdgeNfos(Vector3D vertex, EdgeNfo edgeNfo)
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

                foreach (var thisEdgeNfo in thisLoopBrokenEdgeNfos)
                {
                    thisEdgeNfo.overlapped = other.Overlap(tol, thisEdgeNfo.edge);
                    thisEdgeNfo.occluded = !thisEdgeNfo.overlapped && other.Contains(tol, thisEdgeNfo.edge);

                    updateVertexToEdgeNfos(thisEdgeNfo.edge.SGeomFrom, thisEdgeNfo);
                    updateVertexToEdgeNfos(thisEdgeNfo.edge.SGeomTo, thisEdgeNfo);
                }

                foreach (var otherEdgeNfo in otherLoopBrokenEdgeNfos)
                {
                    otherEdgeNfo.overlapped = this.Overlap(tol, otherEdgeNfo.edge);
                    otherEdgeNfo.occluded = !otherEdgeNfo.overlapped && this.Contains(tol, otherEdgeNfo.edge);

                    if (!otherEdgeNfo.overlapped) // doesn't update other overlapped because will removed later
                    {
                        updateVertexToEdgeNfos(otherEdgeNfo.edge.SGeomFrom, otherEdgeNfo);
                        updateVertexToEdgeNfos(otherEdgeNfo.edge.SGeomTo, otherEdgeNfo);
                    }
                }

                // remove other overlapped edges
                {
                    var qOtherOverlappedEdges = otherLoopBrokenEdgeNfos.Where(edgeNfo => edgeNfo.overlapped).ToList();
                    foreach (var edgeNfoToRemove in qOtherOverlappedEdges) otherLoopBrokenEdgeNfos.Remove(edgeNfoToRemove);
                }
            }

            switch (mode)
            {
                case BooleanMode.Intersect:
                    {
                        var visitedIps = new HashSet<Vector3D>(ptCmp);

                        foreach (var ip in ips)
                        {
                            if (visitedIps.Contains(ip)) continue;
                            if (visitedIps.Count == ips.Count) break;

                            visitedIps.Add(ip);

                            Vector3D lastVertex = ip;
                            EdgeNfo? lastEdgeNfo = null;
                            var loopEdgeNfos = new List<EdgeNfo>();
                            var finished = false;

                            while (!finished)
                            {
                                if (lastEdgeNfo == null) // starting edge
                                {
                                    var edgeNfosFromLastVisitedVertex = vertexToEdgeNfos[lastVertex];

                                    lastEdgeNfo = edgeNfosFromLastVisitedVertex.FirstOrDefault(w => w.occluded || w.overlapped);

                                    if (lastEdgeNfo == null) yield break;
                                }
                                else
                                {
                                    var nextVertex = lastEdgeNfo.edge.OtherEndpoint(tol, lastVertex);

                                    var nextVertexEdges = vertexToEdgeNfos[nextVertex];

                                    var edgeNfosFromLastVisitedVertex = nextVertexEdges
                                        .Where(edgeNfo => (edgeNfo.occluded || edgeNfo.overlapped) && !edgeNfo.edge.EndpointMatches(tol, lastVertex, nextVertex))
                                        .ToList();

                                    if (edgeNfosFromLastVisitedVertex.Count == 0) yield break;

                                    lastEdgeNfo = edgeNfosFromLastVisitedVertex
                                        // give preference on last geom path
                                        .OrderByDescending(w => w.onThis == lastEdgeNfo.onThis)
                                        .First(w => w.occluded || w.overlapped);
                                    lastVertex = nextVertex;

                                    if (lastEdgeNfo.edge.EndpointMatches(tol, ip)) finished = true;
                                }

                                loopEdgeNfos.Add(lastEdgeNfo);
                                if (loopEdgeNfos.Count > thisLoopBrokenEdgeNfos.Count + otherLoopBrokenEdgeNfos.Count)
                                    throw new Exception($"internal error");

                                if (ips.Contains(lastVertex))
                                    visitedIps.Add(lastVertex);
                            }

                            var outerLoop = new Loop(tol, Plane, loopEdgeNfos.Select(w => w.edge).ToList(), checkSense: true);

                            var resFace = new Face(Plane, new[] { outerLoop }.ToList());

                            if (resFace.Area.EqualsTol(tol, 0)) yield break;

                            yield return resFace;
                        }
                    }
                    break;

                case BooleanMode.Difference:
                    {
                        var visitedIps = new HashSet<Vector3D>(ptCmp);

                        foreach (var ip in ips)
                        {
                            if (visitedIps.Contains(ip)) continue;
                            if (visitedIps.Count == ips.Count) break;

                            visitedIps.Add(ip);

                            {
                                var qTypes = vertexToEdgeNfos[ip];
                                var onthis = qTypes.Count(w => w.onThis);
                                if (onthis == 0 || onthis == qTypes.Count)
                                    continue;
                            }

                            Vector3D lastVertex = ip;
                            EdgeNfo? lastEdgeNfo = null;
                            var loopEdgeNfos = new List<EdgeNfo>();
                            var finished = false;
                            var occluded = false; // onThis == !occluded

                            while (!finished)
                            {
                                if (lastEdgeNfo == null) // starting edge
                                {
                                    var edgeNfosFromLastVisitedVertex = vertexToEdgeNfos[lastVertex];

                                    lastEdgeNfo = edgeNfosFromLastVisitedVertex.First(w => w.occluded == occluded && !w.overlapped && w.onThis == !occluded);
                                }
                                else
                                {
                                    var nextVertex = lastEdgeNfo.edge.OtherEndpoint(tol, lastVertex);

                                    var qEdges = vertexToEdgeNfos[nextVertex];

                                    if (qEdges.Any(edgeNfo => edgeNfo.occluded == !occluded && edgeNfo.onThis == occluded))
                                        occluded = !occluded;

                                    var edgeNfosFromLastVisitedVertex = qEdges
                                        .Where(edgeNfo => edgeNfo.occluded == occluded && edgeNfo.onThis == !occluded && !edgeNfo.edge.EndpointMatches(tol, lastVertex))
                                        .ToList();

                                    lastEdgeNfo = edgeNfosFromLastVisitedVertex.First(w => w.occluded == occluded);
                                    lastVertex = nextVertex;

                                    if (lastEdgeNfo.edge.EndpointMatches(tol, ip)) finished = true;
                                }

                                loopEdgeNfos.Add(lastEdgeNfo);
                                if (loopEdgeNfos.Count > thisLoopBrokenEdgeNfos.Count + otherLoopBrokenEdgeNfos.Count)
                                    throw new Exception($"internal error");

                                if (ips.Contains(lastEdgeNfo.edge.SGeomFrom))
                                    visitedIps.Add(lastEdgeNfo.edge.SGeomFrom);

                                if (ips.Contains(lastEdgeNfo.edge.SGeomTo))
                                    visitedIps.Add(lastEdgeNfo.edge.SGeomTo);
                            }

                            var outerLoop = new Loop(tol, Plane, loopEdgeNfos.Select(w => w.edge).ToList(), checkSense: true);

                            var resFace = new Face(Plane, new[] { outerLoop }.ToList());

                            if (resFace.Area.EqualsTol(tol, 0)) yield break;

                            yield return resFace;
                        }
                    }
                    break;
            }

            yield break;
        }

        /// <summary>
        /// states if given edge is contained into this face ( inner loop inside content excluded )
        /// </summary>
        /// <param name="tol"></param>
        /// <param name="edge"></param>                
        public bool Contains(double tol, IEdge edge)
        {
            var containedInAnyOfInners = Loops
                .Skip(1)
                .Any(loop => loop.Contains(tol, edge,
                    SGeomFromTestType: LoopContainsPointMode.InsideExcludedPerimeter,
                    SGeomToTestType: LoopContainsPointMode.InsideExcludedPerimeter,
                    MidPointTestType: LoopContainsPointMode.InsideExcludedPerimeter).Eval(x =>
                    x.SGeomFromTestResult || x.SGeomToTestResult || x.MidPointTestResult
                ));

            if (containedInAnyOfInners) return false;

            return Loops[0].Contains(tol, edge);
        }

        /// <summary>
        /// test if edge overlap any of loop edges of this face
        /// </summary>        
        public bool Overlap(double tol, IEdge edge)
        {
            var containsSGeomFrom = false;
            var containsSGeomTo = false;
            var containsMidpoint = false;
            var testSuccessCnt = 0;
            var matchingLoopEdges = new Dictionary<IEdge, int>();

            foreach (var loop in Loops)
            {
                matchingLoopEdges = new Dictionary<IEdge, int>();

                foreach (var loopEdge in loop.Edges)
                {
                    if (!containsSGeomFrom && loopEdge.EdgeContainsPoint(tol, edge.SGeomFrom))
                    {
                        containsSGeomFrom = true;
                        ++testSuccessCnt;

                        if (matchingLoopEdges.ContainsKey(loopEdge))
                            matchingLoopEdges[loopEdge]++;
                        else
                            matchingLoopEdges.Add(loopEdge, 1);
                    }

                    if (!containsSGeomTo && loopEdge.EdgeContainsPoint(tol, edge.SGeomTo))
                    {
                        containsSGeomTo = true;
                        ++testSuccessCnt;

                        if (matchingLoopEdges.ContainsKey(loopEdge))
                            matchingLoopEdges[loopEdge]++;
                        else
                            matchingLoopEdges.Add(loopEdge, 1);
                    }

                    if (!containsMidpoint && loopEdge.EdgeContainsPoint(tol, edge.MidPoint))
                    {
                        containsMidpoint = true;
                        ++testSuccessCnt;

                        if (matchingLoopEdges.ContainsKey(loopEdge))
                            matchingLoopEdges[loopEdge]++;
                        else
                            matchingLoopEdges.Add(loopEdge, 1);
                    }

                    if (testSuccessCnt == 3)
                    {
                        if (matchingLoopEdges.Count > 1)
                        {
                            foreach (var matchingLoopEdge in matchingLoopEdges.Keys)
                            {
                                if (
                                    matchingLoopEdge.EdgeContainsPoint(tol, edge.SGeomFrom) &&
                                    matchingLoopEdge.EdgeContainsPoint(tol, edge.SGeomTo) &&
                                    matchingLoopEdge.EdgeContainsPoint(tol, edge.MidPoint)) return true;

                            }
                            throw new NotImplementedException($"colinear test for overlap on multiple edges");
                        }
                        return true;
                    }
                }
            }

            return false;
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
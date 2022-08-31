using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using static System.FormattableString;
using System.Diagnostics;
using System.Text;

using netDxf;
using netDxf.Entities;
using Newtonsoft.Json;
using netDxf.Tables;

using SearchAThing;
using static SearchAThing.SciToolkit;

namespace SearchAThing
{

    /// <summary>
    /// Planar face with one (outer) or more loops (inners)
    /// </summary>
    public class Face
    {
        public Plane3D Plane { get; private set; }

        public IReadOnlyList<Loop> Loops { get; private set; }

        /// <summary>
        /// planar face with outer and optional inner loops
        /// </summary>
        /// <param name="plane">plane where loop lies</param>
        /// <param name="loops">loops ( first is the outer )</param>
        public Face(Plane3D plane, IEnumerable<Loop> loops)
        {
            Plane = plane;
            Loops = loops.ToReadOnlyList();
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
                if (_Area is null)
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

            public Edge edge { get; private set; }

            public bool onThis { get; private set; }

            public bool outer => loopOwner.outer;

            /// <summary>
            /// (updated after broken geom)
            /// states if edge not totally on counterpart perimeter but something is inside
            /// </summary>            
            public bool? partiallyInside { get; internal set; } = null;

            /// <summary>
            /// (updated after broken geom)
            /// states if this edge overlapped with counterpart
            /// </summary>            
            public bool? overlapped { get; internal set; } = null;

            public EdgeNfo(LoopNfo loopOwner, Edge edge, bool onThis)
            {
                this.loopOwner = loopOwner;
                this.edge = edge;
                this.onThis = onThis;
            }

            public override string ToString() => $"{(onThis ? "" : "!")}onThis {(loopOwner.outer ? "outer" : "inner")} {(partiallyInside == true ? "" : "!")}prtInside {(overlapped == true ? "" : "!")}overlapped {(loopOwner.outer ? "" : "!")}loopOuter edge:{edge}";
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

            var res = new List<List<Edge>>();

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
                                    var externalOuter = allLoopNfos.First(loopNfo => loopNfo.directParent is null);

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

                IEnumerable<Loop> LoopsOperation(IEnumerable<Loop> loops1, IEnumerable<Loop> loops2, BooleanMode mode)
                {
                    var faces1 = loops1.Select(loop => new Face(Plane, new[] { loop })).ToList();
                    var faces2 = loops2.Select(loop => new Face(Plane, new[] { loop })).ToList();

                    if (faces1.Count == 0 && faces2.Count == 0) yield break;

                    if (faces1.Count == 1 && faces2.Count == 0)
                    {
                        yield return loops1.First();
                        yield break;
                    }

                    if (faces1.Count == 0 && faces2.Count == 1)
                    {
                        yield return loops2.First();
                        yield break;
                    }

                    var firstResultSet = new List<List<Loop>>();

                    var f1loopidx = 0;

                    foreach (var face1 in faces1)
                    {
                        firstResultSet.Add(new List<Loop>());

                        foreach (var face2 in faces2)
                        {
                            var qfaces = face1.Boolean(tol, face2, mode).ToList();

                            foreach (var face in qfaces.Where(face => face.Area > 0))
                            {
                                firstResultSet[f1loopidx].Add(face.Loops[0]);
                            }
                        }

                        ++f1loopidx;
                    }
                    var loopCmp = new LoopEqualityComparer(tol);

                    firstResultSet = firstResultSet.Select(w => w.Distinct(loopCmp).ToList()).ToList();

                    var finalResultSet = new List<List<Face>>();

                    if (mode == BooleanMode.Union)
                    {
                        var subOp = BooleanMode.Union;

                        var faces = firstResultSet.SelectMany(l => l.Select(loop => new Face(Plane, new[] { loop }))).ToList();

                        var resSet = new List<Face>();

                        if (faces.Count == 1)
                            resSet.Add(faces[0]);

                        else
                            for (int i = 0; i < faces.Count; ++i)
                            {
                                if (i == 0)
                                {
                                    resSet = faces[i].Boolean(tol, faces[i + 1], subOp).ToList();
                                    ++i;
                                }
                                else
                                {
                                    var newSet = new List<Face>();

                                    for (int j = 0; j < resSet.Count; ++j)
                                    {
                                        newSet.AddRange(resSet[j].Boolean(tol, faces[i], subOp));
                                    }
                                    newSet = newSet
                                        .Select(face => face.Loops[0])
                                        .Distinct(loopCmp)
                                        .ToList()
                                        .Select(loop => new Face(Plane, new[] { loop })).ToList();

                                    resSet = newSet;
                                }
                            }

                        finalResultSet.Add(resSet);
                    }
                    else
                    {
                        var subOp = BooleanMode.Intersect;

                        foreach (var k in firstResultSet)
                        {
                            var faces = k.Select(loop => new Face(Plane, new[] { loop })).ToList();

                            if (faces.Count == 1)
                                finalResultSet.Add(new List<Face>() { faces[0] });
                            else
                            {
                                var resSet = new List<Face>();

                                for (int i = 0; i < faces.Count; ++i)
                                {
                                    if (i == 0)
                                    {
                                        resSet = faces[i].Boolean(tol, faces[i + 1], subOp).ToList();
                                        ++i;
                                    }
                                    else
                                    {
                                        if (finalResultSet.Count == 0) resSet = new List<Face>() { faces[i] };
                                        else
                                        {
                                            var newSet = new List<Face>();

                                            for (int j = 0; j < finalResultSet.Count; ++j)
                                            {
                                                newSet.AddRange(resSet[j].Boolean(tol, faces[i], subOp));
                                            }
                                            newSet = newSet
                                                .Select(face => face.Loops[0])
                                                .Distinct(loopCmp)
                                                .ToList()
                                                .Select(loop => new Face(Plane, new[] { loop })).ToList();

                                            resSet = newSet;
                                        }
                                    }
                                }

                                finalResultSet.Add(resSet);
                            }
                        }

                    }

                    var allResFaces = finalResultSet.SelectMany(f => f).ToList();

                    if (mode == BooleanMode.Union)
                    {
                        var faceToRemove = new HashSet<Face>();

                        foreach (var face1 in allResFaces)
                        {
                            foreach (var face2 in allResFaces)
                            {
                                if (face1 == face2) continue;

                                if (face1.Loops[0].Edges.All(edge =>
                                    face2.Contains(tol, edge, evalOnlyOuter: true, mode: LoopContainsEdgeMode.InsideOrPerimeter)))
                                    faceToRemove.Add(face1);
                            }
                        }

                        allResFaces = allResFaces.Where(face => !faceToRemove.Contains(face)).ToList();
                    }

                    foreach (var resface in allResFaces)
                        yield return resface.Loops[0];
                }

                #region special case outer equals
                if (thisOuterLoopNfo.loop.Equals(tol, otherOuterLoopNfo.loop))
                {
                    switch (mode)
                    {
                        case BooleanMode.Intersect:
                            {
                                var thisInnerLoops = thisLoopNfos.Where(loopNfo => !loopNfo.outer).Select(w => w.loop).ToList();
                                var otherInnersLoops = otherLoopNfos.Where(loopNfo => !loopNfo.outer).Select(w => w.loop).ToList();

                                var innerLoops = LoopsOperation(thisInnerLoops, otherInnersLoops, BooleanMode.Union).ToList();

                                yield return new Face(Plane, new[] { this.Loops[0] }.Union(innerLoops).ToArray());
                            }
                            break;

                        case BooleanMode.Difference:
                            {
                                if (other.Loops.Count > 1)
                                {
                                    var thisInnerLoops = thisLoopNfos.Where(loopNfo => !loopNfo.outer).Select(w => w.loop).ToList();
                                    var otherInnersLoops = otherLoopNfos.Where(loopNfo => !loopNfo.outer).Select(w => w.loop).ToList();

                                    var innerLoops = LoopsOperation(otherInnersLoops, thisInnerLoops, BooleanMode.Difference).ToList();

                                    foreach (var l in innerLoops)
                                        yield return new Face(Plane, new[] { l });
                                }                                
                            }
                            break;

                        case BooleanMode.Union:
                            {
                                var thisInnerLoops = thisLoopNfos.Where(loopNfo => !loopNfo.outer).Select(w => w.loop).ToList();
                                var otherInnersLoops = otherLoopNfos.Where(loopNfo => !loopNfo.outer).Select(w => w.loop).ToList();

                                var innerLoops = LoopsOperation(thisInnerLoops, otherInnersLoops, BooleanMode.Intersect).ToList();

                                yield return new Face(Plane, new[] { this.Loops[0] }.Union(innerLoops).ToArray());
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
                                .Where(geom => geom.Length.GreatThanTol(tol, 0))
                                .Select(geom => new EdgeNfo(edgeNfo.loopOwner, (Edge)geom, edgeNfo.onThis))
                                .ToArray();
                        }
                        else
                            return new[] { edgeNfo };
                    }).ToList();

                thisLoopBrokenEdgeNfos = BuildBrokenNfos(thisLoopEdgeNfos, thisLoopEdgeNfoToBreaks);
                otherLoopBrokenEdgeNfos = BuildBrokenNfos(otherLoopEdgeNfos, otherLoopEdgeNfoToBreaks);

                #region special case : no edge stricly inside counterpart
                if (!otherLoopBrokenEdgeNfos.Any(otherEdgeNfo => Contains(tol, otherEdgeNfo.edge,
                    evalOnlyOuter: true,
                    LoopContainsEdgeMode.MidPointInside)))
                {
                    switch (mode)
                    {

                        case BooleanMode.Difference:
                            {
                                yield return this;
                                yield break;
                            }

                        case BooleanMode.Union:
                            {
                                var outerEdges = thisLoopBrokenEdgeNfos
                                    .Union(otherLoopBrokenEdgeNfos)
                                    .Select(edgeNfo => edgeNfo.edge)
                                    .ToList();

                                while (true)
                                {
                                    var vertexToEdges = outerEdges.MakeVertexToEdges(tol);

                                    var qStartVertex = vertexToEdges
                                        .Where(vten => vten.Value.Count() > 2)
                                        .Select(vten => vten.Key)
                                        .FirstOrDefault();

                                    if (qStartVertex is null) break;

                                    var shortestEdgePath = vertexToEdges[qStartVertex]
                                        .Select(edge => SciToolkit.WalkEdges(tol,
                                            startEdge: edge,
                                            startVertex: qStartVertex,
                                            vertexToEdges,
                                            stopCondition: (startVertex, nextVertex) =>
                                                vertexToEdges[nextVertex].Count > 2).ToList())
                                        .OrderBy(w => w.Count)
                                        .First();

                                    foreach (var edgeToRemove in shortestEdgePath)
                                        outerEdges.Remove(edgeToRemove);
                                }

                                var outerLoop = new Loop(tol, Plane, outerEdges, checkSense: true, checkSort: true);

                                var qInnerLoops = thisLoopNfos.Where(loopNfo => !loopNfo.outer)
                                    .Union(otherLoopNfos.Where(loopNfo => !loopNfo.outer))
                                    .Select(loopNfo => loopNfo.loop)
                                    .ToList();

                                yield return new Face(Plane, new[] { outerLoop }.Union(qInnerLoops).ToArray());
                                yield break;
                            }

                    }
                }
                #endregion

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
                    thisEdgeNfo.partiallyInside =
                        other.Contains(tol, thisEdgeNfo.edge, evalOnlyOuter: false, LoopContainsEdgeMode.MidPointInside);

                    updateVertexToEdgeNfos(thisEdgeNfo.edge.SGeomFrom, thisEdgeNfo);
                    updateVertexToEdgeNfos(thisEdgeNfo.edge.SGeomTo, thisEdgeNfo);
                }

                foreach (var otherEdgeNfo in otherLoopBrokenEdgeNfos)
                {
                    otherEdgeNfo.overlapped = this.Overlap(tol, otherEdgeNfo.edge);
                    otherEdgeNfo.partiallyInside =
                        this.Contains(tol, otherEdgeNfo.edge, evalOnlyOuter: false, LoopContainsEdgeMode.MidPointInside);

                    if (otherEdgeNfo.overlapped == false) // doesn't update other overlapped because will removed later
                    {
                        updateVertexToEdgeNfos(otherEdgeNfo.edge.SGeomFrom, otherEdgeNfo);
                        updateVertexToEdgeNfos(otherEdgeNfo.edge.SGeomTo, otherEdgeNfo);
                    }
                }

                // remove other overlapped edges
                {
                    var qOtherOverlappedEdges = otherLoopBrokenEdgeNfos.Where(edgeNfo => edgeNfo.overlapped == true).ToList();
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
                                if (lastEdgeNfo is null) // starting edge
                                {
                                    var edgeNfosFromLastVisitedVertex = vertexToEdgeNfos[lastVertex];

                                    lastEdgeNfo = edgeNfosFromLastVisitedVertex
                                        .FirstOrDefault(w => w.partiallyInside == true || w.overlapped == true);

                                    if (lastEdgeNfo is null) yield break;
                                }
                                else
                                {
                                    var nextVertex = lastEdgeNfo.edge.OtherEndpoint(tol, lastVertex);

                                    var nextVertexEdges = vertexToEdgeNfos[nextVertex];

                                    var edgeNfosFromLastVisitedVertex = nextVertexEdges.Where(edgeNfo =>
                                        (edgeNfo.partiallyInside == true || edgeNfo.overlapped == true) &&
                                        !edgeNfo.edge.EndpointMatches(tol, lastVertex, nextVertex))
                                        .ToList();

                                    if (edgeNfosFromLastVisitedVertex.Count == 0) yield break;

                                    lastEdgeNfo = edgeNfosFromLastVisitedVertex
                                        // give preference on last geom path
                                        .OrderByDescending(w => w.onThis == lastEdgeNfo.onThis)
                                        .First(w => w.partiallyInside == true || w.overlapped == true);
                                    lastVertex = nextVertex;

                                    if (lastEdgeNfo.edge.EndpointMatches(tol, ip)) finished = true;
                                }

                                loopEdgeNfos.Add(lastEdgeNfo);
                                if (loopEdgeNfos.Count > thisLoopBrokenEdgeNfos.Count + otherLoopBrokenEdgeNfos.Count)
                                    throw new Exception($"internal error");

                                if (ips.Contains(lastVertex))
                                    visitedIps.Add(lastVertex);
                            }

                            var outerLoop = new Loop(tol, Plane,
                                loopEdgeNfos.Where(edgeNfo => edgeNfo.edge.Length.GreatThanTol(tol, 0))
                                .Select(w => w.edge).ToList(),
                                checkSense: true);

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

                            var edgeNfosFromIp = vertexToEdgeNfos[ip];

                            if (!edgeNfosFromIp.Any(edgeNfo =>
                                edgeNfo.onThis &&
                                edgeNfo.partiallyInside == false &&
                                edgeNfo.overlapped == false))
                                continue;

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
                            var searchInsideEdge = false;
                            var searchOnThis = true;

                            while (!finished)
                            {
                                if (lastEdgeNfo is null) // starting edge
                                {
                                    var edgeNfosFromLastVisitedVertex = vertexToEdgeNfos[lastVertex];

                                    lastEdgeNfo = edgeNfosFromLastVisitedVertex.First(w =>
                                        w.overlapped == false &&
                                        w.partiallyInside == searchInsideEdge &&
                                        w.onThis == searchOnThis);
                                }
                                else
                                {
                                    var nextVertex = lastEdgeNfo.edge.OtherEndpoint(tol, lastVertex);

                                    var qEdges = vertexToEdgeNfos[nextVertex];

                                    if (qEdges.Any(edgeNfo =>
                                        edgeNfo.partiallyInside == !searchInsideEdge &&
                                        edgeNfo.onThis == !searchOnThis))
                                    {
                                        searchInsideEdge = !searchInsideEdge;
                                        searchOnThis = !searchOnThis;
                                    }

                                    var edgeNfosFromLastVisitedVertex = qEdges
                                        .Where(edgeNfo =>                                            
                                            edgeNfo.partiallyInside == searchInsideEdge &&
                                            edgeNfo.onThis == searchOnThis &&
                                            // discard previous
                                            !edgeNfo.edge.EndpointMatches(tol, lastVertex))
                                        .ToList();

                                    lastEdgeNfo = edgeNfosFromLastVisitedVertex
                                        .First(w => w.partiallyInside == searchInsideEdge);
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

                            var innerLoops = thisLoopNfos
                                .Where(loopNfo => outerLoop.Contains(tol, loopNfo.loop, mode: LoopContainsEdgeMode.InsideExcludedPerimeter))
                                .Select(loopNfo => loopNfo.loop)
                                .ToList();

                            var resFace = new Face(Plane, new[] { outerLoop }.Union(innerLoops).ToList());

                            if (resFace.Area.EqualsTol(tol, 0)) yield break;

                            yield return resFace;
                        }
                    }
                    break;

                case BooleanMode.Union:
                    {
                        #region special case only outers
                        if (this.Loops.Count == 1 && other.Loops.Count == 1)
                        {
                            var thisInsideOther = this.Loops[0].Edges.All(edge =>
                                other.Contains(tol, edge, true, LoopContainsEdgeMode.InsideOrPerimeter));

                            if (thisInsideOther)
                            {
                                yield return other;
                                yield break;
                            }

                            var otherInsideThis = other.Loops[0].Edges.All(edge =>
                                this.Contains(tol, edge, true, LoopContainsEdgeMode.InsideOrPerimeter));

                            if (otherInsideThis)
                            {
                                yield return this;
                                yield break;
                            }
                        }
                        #endregion

                        var visitedIps = new HashSet<Vector3D>(ptCmp);
                        Loop? resOuterLoop = null;
                        var resInnerLoops = new List<Loop>();
                        var usedInnerLoops = new HashSet<LoopNfo>();

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
                            var searchInsideEdge = false;
                            var searchOnThis = true;

                            while (!finished)
                            {
                                if (lastEdgeNfo is null) // starting edge
                                {
                                    var edgeNfosFromLastVisitedVertex = vertexToEdgeNfos[lastVertex];

                                    var q = edgeNfosFromLastVisitedVertex.FirstOrDefault(w =>
                                        w.partiallyInside == searchInsideEdge &&
                                        w.overlapped == false &&
                                        w.onThis == searchOnThis);

                                    if (q is null)
                                    {
                                        lastEdgeNfo = edgeNfosFromLastVisitedVertex.FirstOrDefault(w =>
                                            w.partiallyInside == false);
                                    }
                                    else
                                        lastEdgeNfo = q;
                                }
                                else
                                {
                                    var nextVertex = lastEdgeNfo.edge.OtherEndpoint(tol, lastVertex);

                                    var qEdges = vertexToEdgeNfos[nextVertex];

                                    var edgeNfosFromLastVisitedVertex = qEdges.Where(edgeNfo =>                                        
                                        !edgeNfo.edge.EndpointMatches(tol, lastVertex))
                                        .ToList();

                                    var q = edgeNfosFromLastVisitedVertex.FirstOrDefault(w => w.partiallyInside == searchInsideEdge);
                                    if (q is null)
                                    {
                                        q = edgeNfosFromLastVisitedVertex.FirstOrDefault(w => w.partiallyInside == false);

                                        if (q is null)
                                            lastEdgeNfo = edgeNfosFromLastVisitedVertex.First(w => w.overlapped == true);
                                        else
                                            lastEdgeNfo = q;
                                    }
                                    else
                                        lastEdgeNfo = q;

                                    lastVertex = nextVertex;

                                    if (lastEdgeNfo.edge.EndpointMatches(tol, ip)) finished = true;
                                }

                                loopEdgeNfos.Add(lastEdgeNfo!);
                                if (loopEdgeNfos.Count > thisLoopBrokenEdgeNfos.Count + otherLoopBrokenEdgeNfos.Count)
                                    throw new Exception($"internal error");

                                if (ips.Contains(lastEdgeNfo!.edge.SGeomFrom))
                                    visitedIps.Add(lastEdgeNfo.edge.SGeomFrom);

                                if (ips.Contains(lastEdgeNfo.edge.SGeomTo))
                                    visitedIps.Add(lastEdgeNfo.edge.SGeomTo);
                            }

                            if (resOuterLoop is null)
                            {
                                resOuterLoop = new Loop(tol, Plane, loopEdgeNfos.Select(w => w.edge).ToList(), checkSense: true);                                
                            }
                            else
                            {
                                var qUsedInnerLoops = loopEdgeNfos
                                    .Select(edgeNfo => edgeNfo.loopOwner)
                                    .Where(loopNfo => !loopNfo.outer)
                                    .Distinct()
                                    .ToList();

                                foreach (var usedInnerLoop in qUsedInnerLoops) usedInnerLoops.Add(usedInnerLoop);

                                var innerLoop = new Loop(tol, Plane, loopEdgeNfos.Select(w => w.edge).ToList(), checkSense: true);

                                resInnerLoops.Add(innerLoop);
                            }
                        }

                        if (resOuterLoop != null)
                        {
                            var addictionalInnerLoops = thisLoopNfos.Union(otherLoopNfos)
                                .Where(loopNfo => !loopNfo.outer && !usedInnerLoops.Contains(loopNfo))
                                .Select(loopNfo => loopNfo.loop)
                                .ToList();

                            yield return new Face(Plane, new[] { resOuterLoop }.Union(resInnerLoops).Union(addictionalInnerLoops).ToArray());
                        }
                    }
                    break;
            }

            yield break;
        }

        /// <summary>
        /// states if given edge is contained into this face
        /// </summary>
        /// <param name="tol">length tolerance</param>
        /// <param name="edge">edge to test</param>                    
        /// <param name="evalOnlyOuter">if true and edge is contained into and inner the test returns false</param>
        /// <param name="mode">type of contains test</param>                
        public bool Contains(double tol, Edge edge,
            bool evalOnlyOuter,
            LoopContainsEdgeMode mode = LoopContainsEdgeMode.InsideExcludedPerimeter)
        {
            if (!evalOnlyOuter)
            {
                var containedInAnyOfInners = Loops
                    .Skip(1)
                    .Any(loop => loop.Contains(tol, edge, mode));

                if (containedInAnyOfInners) return false;
            }

            return Loops[0].Contains(tol, edge, mode);
        }

        /// <summary>
        /// test if edge overlap any of loop edges of this face
        /// </summary>        
        public bool Overlap(double tol, Edge edge)
        {
            foreach (var loop in Loops)
            {
                foreach (var loopEdge in loop.Edges)
                {
                    if (
                        loopEdge.EdgeContainsPoint(tol, edge.SGeomFrom) &&
                        loopEdge.EdgeContainsPoint(tol, edge.SGeomTo) &&
                        loopEdge.EdgeContainsPoint(tol, edge.MidPoint))
                        return true;
                }
            }
            return false;
        }

        public IEnumerable<Polyline2D> DxfEntities(double tol) => Loops.Select(w => w.DxfEntity(tol));

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

        /// <summary>
        /// rotate cs and edges like point from rotate toward to
        /// </summary>        
        public Face RotateAs(double tol, Vector3D from, Vector3D to)
        {
            var newLoops = new List<Loop>();

            var origin = Plane.CS.Origin;

            foreach (var loop in Loops)
            {
                var newEdges = new List<Edge>();

                foreach (var edge in loop.Edges)
                {
                    switch (edge.GeomType)
                    {
                        case GeometryType.Line3D:
                            {
                                newEdges.Add(
                                    ((edge.SGeomFrom - origin).RotateAs(tol, from, to) + origin).LineTo(
                                    ((edge.SGeomTo - origin).RotateAs(tol, from, to)) + origin));
                            }
                            break;

                        case GeometryType.Arc3D:
                            {
                                newEdges.Add(
                                    new Arc3D(tol,
                                        ((edge.SGeomFrom - origin).RotateAs(tol, from, to) + origin),
                                        ((edge.MidPoint - origin).RotateAs(tol, from, to) + origin),
                                        ((edge.SGeomTo - origin).RotateAs(tol, from, to) + origin)));
                            }
                            break;
                    }
                }

                newLoops.Add(new Loop(tol, newEdges, checkSense: false));
            }

            return new Face(newLoops[0].Plane, newLoops);
        }

        /// <summary>
        /// project this face edges to the given projection plane
        /// </summary>        
        public Face Project(double tol, Plane3D prjPlane)
        {
            var resLoops = new List<Loop>();

            foreach (var loop in Loops)
            {
                var resEdges = new List<Edge>();
                foreach (var edge in loop.Edges)
                {
                    var prjEdge = edge.Project(tol, prjPlane);

                    resEdges.Add(prjEdge);
                }

                resLoops.Add(new Loop(tol, prjPlane, resEdges, checkSense: false));
            }

            return new Face(prjPlane, resLoops);
        }

        public override string ToString() => Invariant($"A:{Area} Loops:{Loops.Count}");

    }

    public static partial class SciExt
    {

        /// <summary>
        /// build planar face with given loops ( first is the outer );
        /// input loop can be unordered ( loop with greather area will be considered as outer loop );
        /// precondition: loops must lie on same plane
        /// </summary>
        public static Face ToFace(this IEnumerable<Polyline2D> lwpolyline, double tol)
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
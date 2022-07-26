using Xunit;
using System.Linq;
using System;
using netDxf;
using netDxf.Entities;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace SearchAThing.Sci.Tests
{
    public partial class LoopTests
    {

        [Fact]
        public void LoopTest_0021()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0021.dxf"));

            var evaltest = false;

            foreach (var op in new[]
            {
                Face.BooleanMode.Intersect,
                // Face.BooleanMode.Difference,
                // Face.BooleanMode.Union
            })
            {

                foreach (var inverseTerm in new[]
                {
                    false,
                    // true
                })
                {
                    DxfDocument? outdxf = null;
                    outdxf = new DxfDocument();

                    for (int layernum = 1; layernum <= 45; ++layernum)
                    {
                        // if (layernum != 45) continue;

                        DxfDocument? outdxf2 = null;
                        // outdxf2 = new DxfDocument();

                        var tol = 1e-8;

                        var layer = dxf.Layers.First(w => w.Name == $"layer {layernum}");

                        var faceGreen = dxf.LwPolylines.Where(w => w.Layer == layer && w.Color.Index == AciColor.Green.Index).ToFace(tol);
                        var faceYellow = dxf.LwPolylines.Where(w => w.Layer == layer && w.Color.Index == AciColor.Yellow.Index).ToFace(tol);
                        // try
                        {
                            outdxf?.AddEntities(faceGreen.Loops.Select(loop => { var ent = loop.DxfEntity(tol).Set(x => x.SetColor(AciColor.Green)); ent.Layer = new netDxf.Tables.Layer(layer.Name); return ent; }));
                            outdxf?.AddEntities(faceYellow.Loops.Select(loop => { var ent = loop.DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow)); ent.Layer = new netDxf.Tables.Layer(layer.Name); return ent; }));

                            var firstTerm = inverseTerm ? faceGreen : faceYellow;
                            var secondTerm = inverseTerm ? faceYellow : faceGreen;

                            var res = firstTerm.Boolean(tol, secondTerm, op, outdxf2).ToList();

                            foreach (var face in res)
                            {
                                var hatch = face
                                    .ToHatch(tol, new HatchPattern(HatchPattern.Line.Name) { Angle = 45, Scale = 0.5 })
                                    ?.Set((eo, isBoundary) =>
                                    {
                                        eo.SetColor(AciColor.Cyan);
                                        eo.SetLayer(new netDxf.Tables.Layer(layer.Name));
                                        eo.Lineweight = isBoundary ? Lineweight.W13 : Lineweight.W0;
                                    });
                                if (hatch != null) outdxf?.AddEntity(hatch);
                            }

                            if (outdxf2 != null)
                            {
                                outdxf2.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                                outdxf2.Viewport.ShowGrid = false;
                                outdxf2.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out2.dxf"));
                            }

                            var assertmsg = $"---> {op} layernum:{layernum}" + (inverseTerm ? " inversed" : "");

                            if (evaltest)
                                switch (op)
                                {
                                    case Face.BooleanMode.Union:
                                        {
                                            switch (layernum)
                                            {
                                                case 1:
                                                    Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    res[1].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    break;

                                                case 2:
                                                case 9:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 200, assertmsg);
                                                    break;

                                                case 3:
                                                case 4:
                                                case 18:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    break;

                                                case 5:
                                                case 12:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 180, assertmsg);
                                                    break;

                                                case 6:
                                                case 13:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 184, assertmsg);
                                                    break;

                                                case 7:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 134, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 2, assertmsg);
                                                    break;

                                                case 8:
                                                    {
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        var areaSet = new[]
                                                        {
                                                            89.37733845,
                                                            110.62266155
                                                        };
                                                        var matchIdx = new HashSet<int>();
                                                        foreach (var f in res)
                                                        {
                                                            var A = f.Loops[0].Area;
                                                            for (int i = 0; i < areaSet.Length; ++i)
                                                            {
                                                                if (matchIdx.Contains(i)) continue;
                                                                if (areaSet[i].EqualsTol(tol, A))
                                                                {
                                                                    matchIdx.Add(i);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        Assert.True(matchIdx.Count == areaSet.Length);
                                                    }
                                                    break;

                                                case 10:
                                                case 11:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                    break;

                                                case 14:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 135.07314181, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 2, assertmsg);
                                                    break;

                                                case 15:
                                                    if (!inverseTerm)
                                                    {
                                                        Assert.True(res.Count == 2 && res[0].Loops.Count == 1 && res[1].Loops.Count == 2);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                        res[1].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                    }
                                                    else
                                                    {
                                                        Assert.True(res.Count == 2 && res[0].Loops.Count == 2 && res[1].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    }
                                                    break;

                                                case 16:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 200, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                    break;

                                                case 17:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                    break;

                                                case 19:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 180, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 67.5, assertmsg);
                                                    break;

                                                case 20:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 184, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 69.75, assertmsg);
                                                    break;

                                                case 21:
                                                    {
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 5);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 134, assertmsg);
                                                        var areaSet = new[]
                                                        {
                                                        8, 2,
                                                        3.5, 3.5
                                                    };
                                                        var matchIdx = new HashSet<int>();
                                                        foreach (var l in res[0].Loops.Skip(1))
                                                        {
                                                            var A = l.Area;
                                                            for (int i = 0; i < areaSet.Length; ++i)
                                                            {
                                                                if (matchIdx.Contains(i)) continue;
                                                                if (areaSet[i].EqualsTol(tol, A))
                                                                {
                                                                    matchIdx.Add(i);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        Assert.True(matchIdx.Count == areaSet.Length);
                                                    }
                                                    break;

                                                case 22:
                                                    if (!inverseTerm)
                                                    {
                                                        Assert.True(res.Count == 2 && res[0].Loops.Count == 1 && res[1].Loops.Count == 2);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 89.37733845, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                        res[1].Loops[1].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                    }
                                                    else
                                                    {
                                                        Assert.True(res.Count == 2 && res[0].Loops.Count == 2 && res[1].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 89.37733845, assertmsg);
                                                    }
                                                    break;

                                                case 23:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 200, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                    break;

                                                case 24:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                    break;

                                                case 25:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                    break;

                                                case 26:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 180, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 77.95766101, assertmsg);
                                                    break;

                                                case 27:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 184, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 79.6426729, assertmsg);
                                                    break;

                                                case 28:
                                                    {
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 5);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 135.07314181, assertmsg);
                                                        var areaSet = new[]
                                                        {
                                                        9.78689196, 2,
                                                        7.20431725,
                                                        7.20431725
                                                    };
                                                        var matchIdx = new HashSet<int>();
                                                        foreach (var l in res[0].Loops.Skip(1))
                                                        {
                                                            var A = l.Area;
                                                            for (int i = 0; i < areaSet.Length; ++i)
                                                            {
                                                                if (matchIdx.Contains(i)) continue;
                                                                if (areaSet[i].EqualsTol(tol, A))
                                                                {
                                                                    matchIdx.Add(i);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        Assert.True(matchIdx.Count == areaSet.Length);
                                                    }
                                                    break;

                                                case 29:
                                                    Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 2));
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                    res[1].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    res[1].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                    break;

                                                case 30:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 3);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 200, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                    res[0].Loops[2].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                    break;

                                                case 31:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                    break;

                                                case 32:
                                                    Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 2));
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                    res[1].Loops[0].Area.AssertEqualsTol(tol, 36, assertmsg);
                                                    res[1].Loops[1].Area.AssertEqualsTol(tol, 25, assertmsg);
                                                    break;

                                                case 33:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 4);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 180, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 67.5, assertmsg);
                                                    res[0].Loops[2].Area.AssertEqualsTol(tol, 67.5, assertmsg);
                                                    res[0].Loops[3].Area.AssertEqualsTol(tol, 9, assertmsg);
                                                    break;

                                                case 34:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 4);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 184, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 69.75, assertmsg);
                                                    res[0].Loops[2].Area.AssertEqualsTol(tol, 69.75, assertmsg);
                                                    res[0].Loops[3].Area.AssertEqualsTol(tol, 7, assertmsg);
                                                    break;

                                                case 35:
                                                    {
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 10);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 134, assertmsg);
                                                        var areaSet = new[]
                                                        {
                                                        13.5,
                                                        8, 2,
                                                        9, 18, 9,
                                                        3.5, 3.5,
                                                        13.5
                                                    };
                                                        var matchIdx = new HashSet<int>();
                                                        foreach (var l in res[0].Loops.Skip(1))
                                                        {
                                                            var A = l.Area;
                                                            for (int i = 0; i < areaSet.Length; ++i)
                                                            {
                                                                if (matchIdx.Contains(i)) continue;
                                                                if (areaSet[i].EqualsTol(tol, A))
                                                                {
                                                                    matchIdx.Add(i);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        Assert.True(matchIdx.Count == areaSet.Length);
                                                    }
                                                    break;

                                                case 36:
                                                    {
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 2));
                                                        int a = 0, b = 1;
                                                        if (inverseTerm) { a = 1; b = 0; }
                                                        res[a].Loops[0].Area.AssertEqualsTol(tol, 89.37733845, assertmsg);
                                                        res[a].Loops[1].Area.AssertEqualsTol(tol, 70.33121551, assertmsg);
                                                        res[b].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                        res[b].Loops[1].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                    }
                                                    break;

                                                case 37:
                                                    {
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 3);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 200, assertmsg);
                                                        int a = 1, b = 2;
                                                        if (inverseTerm) { a = 2; b = 1; }
                                                        res[0].Loops[a].Area.AssertEqualsTol(tol, 70.33121551, assertmsg);
                                                        res[0].Loops[b].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                        break;
                                                    }

                                                case 38:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                    break;

                                                case 39:
                                                    Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 2));
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                    res[1].Loops[0].Area.AssertEqualsTol(tol, 45.82415816, assertmsg);
                                                    res[1].Loops[1].Area.AssertEqualsTol(tol, 33.54687742, assertmsg);
                                                    break;

                                                case 40:
                                                    {
                                                        int a = 1, b = 2;
                                                        if (!inverseTerm) { a = 2; b = 1; }

                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 4);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 180, assertmsg);
                                                        res[0].Loops[a].Area.AssertEqualsTol(tol, 57.04233899, assertmsg);
                                                        res[0].Loops[b].Area.AssertEqualsTol(tol, 77.95766101, assertmsg);
                                                        res[0].Loops[3].Area.AssertEqualsTol(tol, 8.54819966, assertmsg);
                                                    }
                                                    break;

                                                case 41:
                                                    {
                                                        int a = 1, b = 2;
                                                        if (!inverseTerm) { a = 2; b = 1; }

                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 4);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 184, assertmsg);
                                                        res[0].Loops[a].Area.AssertEqualsTol(tol, 59.69336216, assertmsg);
                                                        res[0].Loops[b].Area.AssertEqualsTol(tol, 79.6426729, assertmsg);
                                                        res[0].Loops[3].Area.AssertEqualsTol(tol, 6.73887942, assertmsg);
                                                    }
                                                    break;

                                                case 42:
                                                    {
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 10);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 135.07314181, assertmsg);
                                                        var areaSet = new[]
                                                        {
                                                        4.94404412,
                                                        9.78689196, 2,
                                                        14.90999236, 14.90948884, 14.90999236,
                                                        7.20431725, 7.20431725,
                                                        4.94404412
                                                    };
                                                        var matchIdx = new HashSet<int>();
                                                        foreach (var l in res[0].Loops.Skip(1))
                                                        {
                                                            var A = l.Area;
                                                            for (int i = 0; i < areaSet.Length; ++i)
                                                            {
                                                                if (matchIdx.Contains(i)) continue;
                                                                if (areaSet[i].EqualsTol(tol, A))
                                                                {
                                                                    matchIdx.Add(i);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        Assert.True(matchIdx.Count == areaSet.Length);
                                                    }
                                                    break;

                                                case 43:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 4, assertmsg);
                                                    break;

                                                case 44:
                                                case 45:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 7, assertmsg);
                                                    break;
                                            }
                                        }
                                        break;

                                    case Face.BooleanMode.Intersect:
                                        {
                                            switch (layernum)
                                            {
                                                case 1:
                                                case 2:
                                                case 8:
                                                case 9:
                                                case 15:
                                                case 16:
                                                case 22:
                                                case 23:
                                                case 29:
                                                case 30:
                                                case 32:
                                                case 36:
                                                case 37:
                                                case 39:
                                                    Assert.True(res.Count == 0);
                                                    break;

                                                case 3:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    break;

                                                case 4:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 36, assertmsg);
                                                    break;

                                                case 5:
                                                case 12:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 20, assertmsg);
                                                    break;

                                                case 6:
                                                case 13:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 16, assertmsg);
                                                    break;

                                                case 7:
                                                    Assert.True(res.Count == 2 && res[0].Loops.Count == 1 && res[1].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 20, assertmsg);
                                                    res[1].Loops[0].Area.AssertEqualsTol(tol, 20, assertmsg);
                                                    break;

                                                case 10:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                    break;

                                                case 11:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 45.82415816, assertmsg);
                                                    break;

                                                case 14:
                                                    Assert.True(res.Count == 2 && res[0].Loops.Count == 1 && res[1].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 25.55587576, assertmsg);
                                                    res[1].Loops[0].Area.AssertEqualsTol(tol, 25.55587576, assertmsg);
                                                    break;

                                                case 17:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                    break;

                                                case 18:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 36, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 25, assertmsg);
                                                    break;

                                                case 19:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 6.5, assertmsg);
                                                    break;

                                                case 20:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 4.75, assertmsg);
                                                    break;

                                                case 21:
                                                    Assert.True(res.Count == 4 && res.All(f => f.Loops.Count == 1));
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 5, assertmsg);
                                                    res[1].Loops[0].Area.AssertEqualsTol(tol, 5, assertmsg);
                                                    res[2].Loops[0].Area.AssertEqualsTol(tol, 5, assertmsg);
                                                    res[3].Loops[0].Area.AssertEqualsTol(tol, 5, assertmsg);
                                                    break;

                                                case 24:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                    break;

                                                case 25:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 45.82415816, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 33.54687742, assertmsg);
                                                    break;

                                                case 26:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 6.74067686, assertmsg);
                                                    break;

                                                case 27:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 4.42568874, assertmsg);
                                                    break;

                                                case 28:
                                                    Assert.True(res.Count == 4 && res.All(f => f.Loops.Count == 1));
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 4.61116188, assertmsg);
                                                    res[1].Loops[0].Area.AssertEqualsTol(tol, 4.26371591, assertmsg);
                                                    res[2].Loops[0].Area.AssertEqualsTol(tol, 4.26371591, assertmsg);
                                                    res[3].Loops[0].Area.AssertEqualsTol(tol, 4.61116188, assertmsg);
                                                    break;

                                                case 31:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                    break;

                                                case 33:
                                                case 40:
                                                    Assert.True(res.Count == 2 && res[0].Loops.Count == 1 && res[1].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 1, assertmsg);
                                                    res[1].Loops[0].Area.AssertEqualsTol(tol, 1, assertmsg);
                                                    break;

                                                case 34:
                                                    Assert.True(res.Count == 2 && res[0].Loops.Count == 1 && res[1].Loops.Count == 1);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 0.25, assertmsg);
                                                    res[1].Loops[0].Area.AssertEqualsTol(tol, 0.25, assertmsg);
                                                    break;

                                                case 35:
                                                    Assert.True(res.Count == 8 && res.All(f => f.Loops.Count == 1));
                                                    foreach (var f in res) f.Loops[0].Area.AssertEqualsTol(tol, 0.25, assertmsg);
                                                    break;

                                                case 38:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                    break;

                                                case 41:
                                                    {
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        var areaSet = new[]
                                                        {
                                                        0.26253374,
                                                        0.26418108
                                                    };
                                                        var matchIdx = new HashSet<int>();
                                                        foreach (var f in res)
                                                        {
                                                            var A = f.Loops[0].Area;
                                                            for (int i = 0; i < areaSet.Length; ++i)
                                                            {
                                                                if (matchIdx.Contains(i)) continue;
                                                                if (areaSet[i].EqualsTol(tol, A))
                                                                {
                                                                    matchIdx.Add(i);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        Assert.True(matchIdx.Count == areaSet.Length);
                                                    }
                                                    break;

                                                case 42:
                                                    {
                                                        Assert.True(res.Count == 8 && res.All(f => f.Loops.Count == 1));
                                                        var areaSet = new[]
                                                        {
                                                        0.25266566, 0.26291702,
                                                        0.25, 0.25246826,
                                                        0.25, 0.25246826,
                                                        0.25266566, 0.26291702
                                                    };
                                                        var matchIdx = new HashSet<int>();
                                                        foreach (var f in res)
                                                        {
                                                            var A = f.Loops[0].Area;
                                                            for (int i = 0; i < areaSet.Length; ++i)
                                                            {
                                                                if (matchIdx.Contains(i)) continue;
                                                                if (areaSet[i].EqualsTol(tol, A))
                                                                {
                                                                    matchIdx.Add(i);
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        Assert.True(matchIdx.Count == areaSet.Length);
                                                    }
                                                    break;

                                                case 43:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 56.5, assertmsg);
                                                    break;

                                                case 44:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 65.25, assertmsg);
                                                    break;

                                                case 45:
                                                    Assert.True(res.Count == 1 && res[0].Loops.Count == 3);
                                                    res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                    res[0].Loops[1].Area.AssertEqualsTol(tol, 61.75, assertmsg);
                                                    res[0].Loops[2].Area.AssertEqualsTol(tol, 5.5, assertmsg);
                                                    break;
                                            }
                                        }
                                        break;

                                    case Face.BooleanMode.Difference:
                                        {
                                            if (!inverseTerm)
                                                switch (layernum)
                                                {
                                                    case 1:
                                                    case 2:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                        break;

                                                    case 3:
                                                    case 10:
                                                    case 31:
                                                    case 38:
                                                        Assert.True(res.Count == 0);
                                                        break;

                                                    case 4:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 36, assertmsg);
                                                        break;

                                                    case 5:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 80, assertmsg);
                                                        break;

                                                    case 6:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 84, assertmsg);
                                                        break;

                                                    case 7:
                                                        Assert.True(res.Count == 3 && res.All(f => f.Loops.Count == 1));
                                                        foreach (var f in res) f.Loops[0].Area.AssertEqualsTol(tol, 20, assertmsg);
                                                        break;

                                                    case 8:
                                                    case 9:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 89.37733845, assertmsg);
                                                        break;

                                                    case 11:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 45.82415816, assertmsg);
                                                        break;

                                                    case 12:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 69.37733845, assertmsg);
                                                        break;

                                                    case 13:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 73.37733845, assertmsg);
                                                        break;

                                                    case 14:
                                                        {
                                                            Assert.True(res.Count == 3 && res.All(f => f.Loops.Count == 1));
                                                            var areaSet = new[]
                                                            {
                                                                10.67702007,
                                                                16.91154678,
                                                                10.67702007
                                                            };
                                                            var matchIdx = new HashSet<int>();
                                                            foreach (var f in res)
                                                            {
                                                                var A = f.Loops[0].Area;
                                                                for (int i = 0; i < areaSet.Length; ++i)
                                                                {
                                                                    if (matchIdx.Contains(i)) continue;
                                                                    if (areaSet[i].EqualsTol(tol, A))
                                                                    {
                                                                        matchIdx.Add(i);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            Assert.True(matchIdx.Count == areaSet.Length);
                                                        }
                                                        break;

                                                    case 15:
                                                    case 16:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                        break;

                                                    case 17:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                        break;

                                                    case 18:
                                                        Assert.True(res.Count == 2 && res[0].Loops.Count == 2 && res[1].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 36, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 25, assertmsg);
                                                        break;

                                                    case 19:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 80, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 13.5, assertmsg);
                                                        break;

                                                    case 20:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 84, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 11.25, assertmsg);
                                                        break;

                                                    case 21:
                                                        {
                                                            Assert.True(res.Count == 5 && res.All(f => f.Loops.Count == 1));
                                                            var areaSet = new[]
                                                            {
                                                                20d,
                                                                10,
                                                                20,
                                                                10,
                                                                20
                                                            };
                                                            var matchIdx = new HashSet<int>();
                                                            foreach (var f in res)
                                                            {
                                                                var A = f.Loops[0].Area;
                                                                for (int i = 0; i < areaSet.Length; ++i)
                                                                {
                                                                    if (matchIdx.Contains(i)) continue;
                                                                    if (areaSet[i].EqualsTol(tol, A))
                                                                    {
                                                                        matchIdx.Add(i);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            Assert.True(matchIdx.Count == areaSet.Length);
                                                        }
                                                        break;

                                                    case 22:
                                                    case 23:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 89.37733845, assertmsg);
                                                        break;

                                                    case 24:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                        break;

                                                    case 25:
                                                        Assert.True(res.Count == 2 && res[0].Loops.Count == 2 && res[1].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 45.82415816, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 33.54687742, assertmsg);
                                                        break;

                                                    case 26:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 69.37733845, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 13.25932314, assertmsg);
                                                        break;

                                                    case 27:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 73.37733845, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 11.57431126, assertmsg);
                                                        break;

                                                    case 28:
                                                        {
                                                            Assert.True(res.Count == 5 && res.All(f => f.Loops.Count == 1));
                                                            var areaSet = new[]
                                                            {
                                                                10.67702007,
                                                                16.68099798,
                                                                16.91154678,
                                                                16.68099798,
                                                                10.67702007
                                                            };
                                                            var matchIdx = new HashSet<int>();
                                                            foreach (var f in res)
                                                            {
                                                                var A = f.Loops[0].Area;
                                                                for (int i = 0; i < areaSet.Length; ++i)
                                                                {
                                                                    if (matchIdx.Contains(i)) continue;
                                                                    if (areaSet[i].EqualsTol(tol, A))
                                                                    {
                                                                        matchIdx.Add(i);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            Assert.True(matchIdx.Count == areaSet.Length);
                                                        }
                                                        break;

                                                    case 29:
                                                    case 30:
                                                    case 32:
                                                        Assert.True(res.Count == 1 && res.All(f => f.Loops.Count == 2));
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                        break;

                                                    case 33:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 12.5, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 4.5, assertmsg);
                                                        break;

                                                    case 34:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 14.25, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 4.25, assertmsg);
                                                        break;

                                                    case 35:
                                                        {
                                                            Assert.True(res.Count == 8 && res.All(f => f.Loops.Count == 1));
                                                            var areaSet = new[]
                                                            {
                                                            6.5,
                                                            0.5, 0.5,
                                                            1, 1,
                                                            0.5, 0.5,
                                                            6.5,
                                                        };
                                                            var matchIdx = new HashSet<int>();
                                                            foreach (var f in res)
                                                            {
                                                                var A = f.Loops[0].Area;
                                                                for (int i = 0; i < areaSet.Length; ++i)
                                                                {
                                                                    if (matchIdx.Contains(i)) continue;
                                                                    if (areaSet[i].EqualsTol(tol, A))
                                                                    {
                                                                        matchIdx.Add(i);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            Assert.True(matchIdx.Count == areaSet.Length);
                                                        }
                                                        break;

                                                    case 36:
                                                    case 37:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 89.37733845, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 70.33121551, assertmsg);
                                                        break;

                                                    case 39:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                        break;

                                                    case 40:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 12.33499946, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 4.71112349, assertmsg);
                                                        break;

                                                    case 41:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 13.68397629, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 4.83543184, assertmsg);
                                                        break;

                                                    case 42:
                                                        {
                                                            Assert.True(res.Count == 8 && res.All(f => f.Loops.Count == 1));
                                                            var areaSet = new[]
                                                            {
                                                            5.73297594,
                                                            0.80914645, 0.96185917,
                                                            1, 1.00205794,
                                                            0.80914645, 0.96185917,
                                                            5.73297594
                                                        };
                                                            var matchIdx = new HashSet<int>();
                                                            foreach (var f in res)
                                                            {
                                                                var A = f.Loops[0].Area;
                                                                for (int i = 0; i < areaSet.Length; ++i)
                                                                {
                                                                    if (matchIdx.Contains(i)) continue;
                                                                    if (areaSet[i].EqualsTol(tol, A))
                                                                    {
                                                                        matchIdx.Add(i);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            Assert.True(matchIdx.Count == areaSet.Length);
                                                        }
                                                        break;

                                                    case 43:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 26.25, assertmsg);
                                                        break;

                                                    case 44:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 28.75, assertmsg);
                                                        break;

                                                    case 45:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 23.25, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 5.5, assertmsg);
                                                        break;

                                                }
                                            else
                                                switch (layernum)
                                                {
                                                    case 1:
                                                    case 2:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                        break;

                                                    case 3:
                                                    case 4:
                                                    case 10:
                                                    case 11:
                                                    case 17:
                                                    case 18:
                                                    case 24:
                                                    case 25:
                                                    case 31:
                                                    case 38:
                                                        Assert.True(res.Count == 0);
                                                        break;

                                                    case 5:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 80, assertmsg);
                                                        break;

                                                    case 6:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 84, assertmsg);
                                                        break;

                                                    case 7:
                                                        {
                                                            Assert.True(res.Count == 3 && res.All(f => f.Loops.Count == 1));
                                                            var areaSet = new[]
                                                            {
                                                            16d,
                                                            8,
                                                            8
                                                        };
                                                            var matchIdx = new HashSet<int>();
                                                            foreach (var f in res)
                                                            {
                                                                var A = f.Loops[0].Area;
                                                                for (int i = 0; i < areaSet.Length; ++i)
                                                                {
                                                                    if (matchIdx.Contains(i)) continue;
                                                                    if (areaSet[i].EqualsTol(tol, A))
                                                                    {
                                                                        matchIdx.Add(i);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            Assert.True(matchIdx.Count == areaSet.Length);
                                                        }
                                                        break;

                                                    case 8:
                                                    case 9:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                        break;

                                                    case 12:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 90.62266155, assertmsg);
                                                        break;

                                                    case 13:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 94.62266155, assertmsg);
                                                        break;

                                                    case 14:
                                                        {
                                                            Assert.True(res.Count == 3 && res.All(f => f.Loops.Count == 1));
                                                            var areaSet = new[]
                                                            {
                                                            17.89058773,
                                                            12.90260782,
                                                            12.90260782
                                                        };
                                                            var matchIdx = new HashSet<int>();
                                                            foreach (var f in res)
                                                            {
                                                                var A = f.Loops[0].Area;
                                                                for (int i = 0; i < areaSet.Length; ++i)
                                                                {
                                                                    if (matchIdx.Contains(i)) continue;
                                                                    if (areaSet[i].EqualsTol(tol, A))
                                                                    {
                                                                        matchIdx.Add(i);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            Assert.True(matchIdx.Count == areaSet.Length);
                                                        }
                                                        break;

                                                    case 15:
                                                    case 16:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                        break;

                                                    case 19:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 12.5, assertmsg);
                                                        break;

                                                    case 20:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 14.25, assertmsg);
                                                        break;

                                                    case 21:
                                                        {
                                                            Assert.True(res.Count == 4 && res.All(f => f.Loops.Count == 1));
                                                            var areaSet = new[]
                                                            {
                                                            5.5,
                                                            2.5,
                                                            4.5,
                                                            4.5
                                                        };
                                                            var matchIdx = new HashSet<int>();
                                                            foreach (var f in res)
                                                            {
                                                                var A = f.Loops[0].Area;
                                                                for (int i = 0; i < areaSet.Length; ++i)
                                                                {
                                                                    if (matchIdx.Contains(i)) continue;
                                                                    if (areaSet[i].EqualsTol(tol, A))
                                                                    {
                                                                        matchIdx.Add(i);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            Assert.True(matchIdx.Count == areaSet.Length);
                                                        }
                                                        break;

                                                    case 22:
                                                    case 23:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                        break;

                                                    case 26:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 12.66500054, assertmsg);
                                                        break;

                                                    case 27:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 14.97998866, assertmsg);
                                                        break;

                                                    case 28:
                                                        {
                                                            Assert.True(res.Count == 4 && res.All(f => f.Loops.Count == 1));
                                                            var areaSet = new[]
                                                            {
                                                            5.60369576,
                                                            2.5,
                                                            5.69829057,
                                                            5.69829057
                                                        };
                                                            var matchIdx = new HashSet<int>();
                                                            foreach (var f in res)
                                                            {
                                                                var A = f.Loops[0].Area;
                                                                for (int i = 0; i < areaSet.Length; ++i)
                                                                {
                                                                    if (matchIdx.Contains(i)) continue;
                                                                    if (areaSet[i].EqualsTol(tol, A))
                                                                    {
                                                                        matchIdx.Add(i);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            Assert.True(matchIdx.Count == areaSet.Length);
                                                        }
                                                        break;

                                                    case 29:
                                                    case 30:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 100, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 81, assertmsg);
                                                        break;

                                                    case 32:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 36, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 25, assertmsg);
                                                        break;

                                                    case 33:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 4.5, assertmsg);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 12.5, assertmsg);
                                                        break;

                                                    case 34:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 4.25, assertmsg);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 14.25, assertmsg);
                                                        break;

                                                    case 35:
                                                        {
                                                            Assert.True(res.Count == 8 && res.All(f => f.Loops.Count == 1));
                                                            var areaSet = new[]
                                                            {
                                                            5.5, 2.5,
                                                            4.5, 4.5, 4.5, 4.5,
                                                            4.5, 4.5
                                                        };
                                                            var matchIdx = new HashSet<int>();
                                                            foreach (var f in res)
                                                            {
                                                                var A = f.Loops[0].Area;
                                                                for (int i = 0; i < areaSet.Length; ++i)
                                                                {
                                                                    if (matchIdx.Contains(i)) continue;
                                                                    if (areaSet[i].EqualsTol(tol, A))
                                                                    {
                                                                        matchIdx.Add(i);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            Assert.True(matchIdx.Count == areaSet.Length);
                                                        }
                                                        break;

                                                    case 36:
                                                    case 37:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 110.62266155, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 91.21698415, assertmsg);
                                                        break;

                                                    case 39:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 2);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 45.82415816, assertmsg);
                                                        res[0].Loops[1].Area.AssertEqualsTol(tol, 33.54687742, assertmsg);
                                                        break;

                                                    case 40:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 4.74067686, assertmsg);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 12.66500054, assertmsg);
                                                        break;

                                                    case 41:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 3.89897393, assertmsg);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 14.97998866, assertmsg);
                                                        break;

                                                    case 42:
                                                        {
                                                            Assert.True(res.Count == 8 && res.All(f => f.Loops.Count == 1));
                                                            var areaSet = new[]
                                                            {
                                                            5.60369576,
                                                            2.5,
                                                            4.0955792,
                                                            3.76124764,
                                                            3.76124764,
                                                            4.0955792,
                                                            5.69829057,
                                                            5.69829057
                                                        };
                                                            var matchIdx = new HashSet<int>();
                                                            foreach (var f in res)
                                                            {
                                                                var A = f.Loops[0].Area;
                                                                for (int i = 0; i < areaSet.Length; ++i)
                                                                {
                                                                    if (matchIdx.Contains(i)) continue;
                                                                    if (areaSet[i].EqualsTol(tol, A))
                                                                    {
                                                                        matchIdx.Add(i);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            Assert.True(matchIdx.Count == areaSet.Length);
                                                        }
                                                        break;

                                                    case 43:
                                                        Assert.True(res.Count == 1 && res[0].Loops.Count == 1);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 26.25, assertmsg);
                                                        break;

                                                    case 44:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 12.25, assertmsg);
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 17.25, assertmsg);
                                                        break;

                                                    case 45:
                                                        Assert.True(res.Count == 2 && res.All(f => f.Loops.Count == 1));
                                                        res[1].Loops[0].Area.AssertEqualsTol(tol, 12.25, assertmsg);
                                                        res[0].Loops[0].Area.AssertEqualsTol(tol, 19.25, assertmsg);
                                                        break;

                                                }
                                        }
                                        break;
                                }

                            //Assert.True(res.Count == 0);   

                            var outfilename = $"out-{op}-{(inverseTerm ? "GY" : "YG")}.dxf";

                            if (outdxf != null)
                            {
                                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                                outdxf.Viewport.ShowGrid = false;
                                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), outfilename));
                            }
                        }
                        // catch (Exception ex)
                        // {
                        //     outdxf?.AddEntities(faceGreen.Loops.Select(loop => { var ent = loop.DxfEntity(tol).Set(x => x.SetColor(AciColor.Red)); ent.Layer = new netDxf.Tables.Layer(layer.Name); return ent; }));
                        //     outdxf?.AddEntities(faceYellow.Loops.Select(loop => { var ent = loop.DxfEntity(tol).Set(x => x.SetColor(AciColor.Red)); ent.Layer = new netDxf.Tables.Layer(layer.Name); return ent; }));

                        // }
                    }

;
                }

            }

        }

    }

}
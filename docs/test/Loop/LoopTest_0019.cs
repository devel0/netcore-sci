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
        public void LoopTest_0019()
        {            
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0019.dxf"));

            foreach (var op in new[] { Face.BooleanMode.Union, Face.BooleanMode.Intersect, Face.BooleanMode.Difference })
            {
                foreach (var inverseTerm in new[] { false, true })
                {
                    for (int layernum = 1; layernum <= 12; ++layernum)
                    {

                        DxfDocument? outdxf = null;
                        // outdxf = new DxfDocument();

                        DxfDocument? outdxf2 = null;
                        // outdxf2 = new DxfDocument();

                        var tol = 1e-8;

                        var layer = dxf.Layers.First(w => w.Name == $"layer {layernum}");

                        var faceGreen = dxf.LwPolylines.Where(w => w.Layer == layer && w.Color.Index == AciColor.Green.Index).ToFace(tol);
                        var faceYellow = dxf.LwPolylines.Where(w => w.Layer == layer && w.Color.Index == AciColor.Yellow.Index).ToFace(tol);

                        outdxf?.AddEntities(faceGreen.Loops.Select(loop => loop.DxfEntity(tol).Set(x => x.SetColor(AciColor.Green))));
                        outdxf?.AddEntities(faceYellow.Loops.Select(loop => loop.DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow))));

                        var layerRes = new netDxf.Tables.Layer("res") { Color = AciColor.Cyan };

                        var firstTerm = inverseTerm ? faceGreen : faceYellow;
                        var secondTerm = inverseTerm ? faceYellow : faceGreen;

                        var res = firstTerm.Boolean(tol, secondTerm, op, outdxf2).ToList();

                        foreach (var face in res)
                        {
                            var hatch = face
                                .ToHatch(tol, new HatchPattern(HatchPattern.Line.Name) { Angle = 45, Scale = 0.5 })
                                ?.Set((eo, isBoundary) =>
                                {
                                    eo.SetLayer(layerRes);
                                    eo.Lineweight = isBoundary ? Lineweight.W13 : Lineweight.W0;
                                });
                            if (hatch != null) outdxf?.AddEntity(hatch);
                        }

                        if (outdxf != null)
                        {
                            outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                            outdxf.Viewport.ShowGrid = false;
                            outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
                        }

                        if (outdxf2 != null)
                        {
                            outdxf2.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                            outdxf2.Viewport.ShowGrid = false;
                            outdxf2.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out2.dxf"));
                        }

                        var assertmsg = $"---> {op} layernum:{layernum}" + (inverseTerm ? " inversed" : "");

                        switch (op)
                        {
                            case Face.BooleanMode.Union:
                                {
                                    switch (layernum)
                                    {
                                        case 1:
                                        case 2:
                                            Assert.True(res.Count == 1 && res[0].Loops.Count == 1, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 900, assertmsg);
                                            break;

                                        case 3:
                                            Assert.True(res.Count == 2 && res[0].Loops.Count == 2 && res[1].Loops.Count == 1, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 900, assertmsg);
                                            res[0].Loops[1].Area.AssertEqualsTol(tol, 576, assertmsg);
                                            res[1].Loops[0].Area.AssertEqualsTol(tol, 196, assertmsg);
                                            break;

                                        case 4:
                                            Assert.True(res.Count == 2 && res[0].Loops.Count == 2 && res[1].Loops.Count == 2, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 900, assertmsg);
                                            res[0].Loops[1].Area.AssertEqualsTol(tol, 576, assertmsg);
                                            res[1].Loops[0].Area.AssertEqualsTol(tol, 196, assertmsg);
                                            res[1].Loops[1].Area.AssertEqualsTol(tol, 64, assertmsg);
                                            break;

                                        case 5:
                                        case 6:
                                            Assert.True(res.Count == 1 && res[0].Loops.Count == 2, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 900, assertmsg);
                                            res[0].Loops[1].Area.AssertEqualsTol(tol, 64, assertmsg);
                                            break;

                                        case 7:
                                        case 8:
                                        case 9:
                                            Assert.True(res.Count == 1 && res[0].Loops.Count == 1, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 1500, assertmsg);
                                            break;

                                        case 10:
                                            Assert.True(res.Count == 1 && res[0].Loops.Count == 3, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 1500, assertmsg);
                                            res[0].Loops[1].Area.AssertEqualsTol(tol, 64, assertmsg);
                                            res[0].Loops[2].Area.AssertEqualsTol(tol, 64, assertmsg);
                                            break;

                                        case 11:
                                            Assert.True(res.Count == 2 && res[0].Loops.Count == 3 && res[1].Loops.Count == 1, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 1800, assertmsg);
                                            res[0].Loops[1].Area.AssertEqualsTol(tol, 576, assertmsg);
                                            res[0].Loops[2].Area.AssertEqualsTol(tol, 576, assertmsg);
                                            res[1].Loops[0].Area.AssertEqualsTol(tol, 64, assertmsg);
                                            break;

                                        case 12:
                                            Assert.True(res.Count == 2 && res[0].Loops.Count == 3 && res[1].Loops.Count == 2, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 1800, assertmsg);
                                            res[0].Loops[1].Area.AssertEqualsTol(tol, 576, assertmsg);
                                            res[0].Loops[2].Area.AssertEqualsTol(tol, 576, assertmsg);
                                            res[1].Loops[0].Area.AssertEqualsTol(tol, 196, assertmsg);
                                            res[1].Loops[1].Area.AssertEqualsTol(tol, 64, assertmsg);
                                            break;
                                    }
                                }
                                break;

                            case Face.BooleanMode.Intersect:
                                {
                                    switch (layernum)
                                    {
                                        case 1:
                                            Assert.True(res.Count == 1 && res[0].Loops.Count == 1, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 196, assertmsg);
                                            break;

                                        case 2:
                                            Assert.True(res.Count == 1 && res[0].Loops.Count == 2, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 196, assertmsg);
                                            res[0].Loops[1].Area.AssertEqualsTol(tol, 64, assertmsg);
                                            break;

                                        case 3:
                                        case 4:
                                        case 11:
                                        case 12:
                                            Assert.True(res.Count == 0, assertmsg);
                                            break;

                                        case 5:
                                        case 6:
                                            Assert.True(res.Count == 1 && res[0].Loops.Count == 2, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 576, assertmsg);
                                            res[0].Loops[1].Area.AssertEqualsTol(tol, 196, assertmsg);
                                            break;

                                        case 7:
                                            Assert.True(res.Count == 1 && res[0].Loops.Count == 2, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 476, assertmsg);
                                            res[0].Loops[1].Area.AssertEqualsTol(tol, 64, assertmsg);
                                            break;

                                        case 8:
                                        case 9:
                                            Assert.True(res.Count == 1 && res[0].Loops.Count == 3, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 476, assertmsg);
                                            res[0].Loops[1].Area.AssertEqualsTol(tol, 64, assertmsg);
                                            res[0].Loops[2].Area.AssertEqualsTol(tol, 64, assertmsg);
                                            break;

                                        case 10:
                                            Assert.True(res.Count == 1 && res[0].Loops.Count == 2, assertmsg);
                                            res[0].Loops[0].Area.AssertEqualsTol(tol, 840, assertmsg);
                                            res[0].Loops[1].Area.AssertEqualsTol(tol, 476, assertmsg);
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
                                                Assert.True(res.Count == 1 && res[0].Loops.Count == 2, assertmsg);
                                                res[0].Loops[0].Area.AssertEqualsTol(tol, 900, assertmsg);
                                                res[0].Loops[1].Area.AssertEqualsTol(tol, 196, assertmsg);
                                                break;

                                            case 2:
                                                Assert.True(res.Count == 2 && res[0].Loops.Count == 2 && res[1].Loops.Count == 1, assertmsg);
                                                res[0].Loops[0].Area.AssertEqualsTol(tol, 900, assertmsg);
                                                res[0].Loops[1].Area.AssertEqualsTol(tol, 196, assertmsg);
                                                res[1].Loops[0].Area.AssertEqualsTol(tol, 64, assertmsg);
                                                break;

                                            case 3:
                                            case 4:
                                            case 6:
                                                Assert.True(res.Count == 1 && res[0].Loops.Count == 2, assertmsg);
                                                res[0].Loops[0].Area.AssertEqualsTol(tol, 900, assertmsg);
                                                res[0].Loops[1].Area.AssertEqualsTol(tol, 576, assertmsg);
                                                break;

                                            case 5:
                                                Assert.True(res.Count == 2 && res[0].Loops.Count == 2 && res[1].Loops.Count == 2, assertmsg);
                                                res[0].Loops[0].Area.AssertEqualsTol(tol, 900, assertmsg);
                                                res[0].Loops[1].Area.AssertEqualsTol(tol, 576, assertmsg);
                                                res[1].Loops[0].Area.AssertEqualsTol(tol, 196, assertmsg);
                                                res[1].Loops[1].Area.AssertEqualsTol(tol, 64, assertmsg);
                                                break;

                                            case 7:
                                                Assert.True(res.Count == 2 && res[0].Loops.Count == 2 && res[1].Loops.Count == 1, assertmsg);
                                                res[0].Loops[0].Area.AssertEqualsTol(tol, 1500, assertmsg);
                                                res[0].Loops[1].Area.AssertEqualsTol(tol, 476, assertmsg);
                                                res[1].Loops[0].Area.AssertEqualsTol(tol, 64, assertmsg);
                                                break;

                                            case 8:
                                                Assert.True(res.Count == 3 && res[0].Loops.Count == 2 && res[1].Loops.Count == 1 && res[2].Loops.Count == 1, assertmsg);
                                                res[0].Loops[0].Area.AssertEqualsTol(tol, 1500, assertmsg);
                                                res[0].Loops[1].Area.AssertEqualsTol(tol, 476, assertmsg);
                                                res[1].Loops[0].Area.AssertEqualsTol(tol, 64, assertmsg);
                                                res[2].Loops[0].Area.AssertEqualsTol(tol, 64, assertmsg);
                                                break;

                                            case 9:
                                                Assert.True(res.Count == 2 && res[0].Loops.Count == 2 && res[1].Loops.Count == 1, assertmsg);
                                                res[0].Loops[0].Area.AssertEqualsTol(tol, 1500, assertmsg);
                                                res[0].Loops[1].Area.AssertEqualsTol(tol, 476, assertmsg);
                                                res[1].Loops[0].Area.AssertEqualsTol(tol, 64, assertmsg);
                                                break;

                                            case 10:
                                                Assert.True(res.Count == 1 && res[0].Loops.Count == 2, assertmsg);
                                                res[0].Loops[0].Area.AssertEqualsTol(tol, 1500, assertmsg);
                                                res[0].Loops[1].Area.AssertEqualsTol(tol, 840, assertmsg);
                                                break;

                                            case 11:
                                            case 12:
                                                Assert.True(res.Count == 1 && res[0].Loops.Count == 3, assertmsg);
                                                res[0].Loops[0].Area.AssertEqualsTol(tol, 1800, assertmsg);
                                                res[0].Loops[1].Area.AssertEqualsTol(tol, 576, assertmsg);
                                                res[0].Loops[2].Area.AssertEqualsTol(tol, 576, assertmsg);
                                                break;
                                        }
                                    else
                                        switch (layernum)
                                        {
                                            case 1:
                                            case 2:
                                            case 5:
                                            case 7:
                                            case 8:
                                                Assert.True(res.Count == 0, assertmsg);
                                                break;

                                            case 3:
                                                Assert.True(res.Count == 1 && res[0].Loops.Count == 1, assertmsg);
                                                res[0].Loops[0].Area.AssertEqualsTol(tol, 196, assertmsg);
                                                break;

                                            case 4:
                                            case 6:
                                            case 12:
                                                Assert.True(res.Count == 1 && res[0].Loops.Count == 2, assertmsg);
                                                res[0].Loops[0].Area.AssertEqualsTol(tol, 196, assertmsg);
                                                res[0].Loops[1].Area.AssertEqualsTol(tol, 64, assertmsg);
                                                break;

                                            case 10:
                                                Assert.True(res.Count == 1 && res[0].Loops.Count == 3, assertmsg);
                                                res[0].Loops[0].Area.AssertEqualsTol(tol, 476, assertmsg);
                                                res[0].Loops[1].Area.AssertEqualsTol(tol, 64, assertmsg);
                                                res[0].Loops[2].Area.AssertEqualsTol(tol, 64, assertmsg);
                                                break;

                                            case 9:
                                            case 11:
                                                Assert.True(res.Count == 1 && res[0].Loops.Count == 1, assertmsg);
                                                res[0].Loops[0].Area.AssertEqualsTol(tol, 64, assertmsg);
                                                break;
                                        }
                                }
                                break;
                        }

                        //Assert.True(res.Count == 0);


                    }

                }

            }

        }

    }

}
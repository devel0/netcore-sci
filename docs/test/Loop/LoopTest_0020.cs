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
        public void LoopTest_0020()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0020.dxf"));

            foreach (var op in new[] { Face.BooleanMode.Union, Face.BooleanMode.Intersect, Face.BooleanMode.Difference })
            {
                foreach (var inverseTerm in new[] { false, true })
                {

                    DxfDocument? outdxf = null;
                    // outdxf = new DxfDocument();

                    DxfDocument? outdxf2 = null;
                    // outdxf2 = new DxfDocument();

                    var tol = 1e-8;

                    //var layer = dxf.Layers.First(w => w.Name == $"layer {layernum}");

                    var faceGreen = dxf.Entities.Polylines2D.Where(w => w.Color.Index == AciColor.Green.Index).ToFace(tol);
                    var faceYellow = dxf.Entities.Polylines2D.Where(w => w.Color.Index == AciColor.Yellow.Index).ToFace(tol);

                    outdxf?.AddEntities(faceGreen.Loops.Select(loop => loop.DxfEntity(tol).Act(x => x.SetColor(AciColor.Green))));
                    outdxf?.AddEntities(faceYellow.Loops.Select(loop => loop.DxfEntity(tol).Act(x => x.SetColor(AciColor.Yellow))));

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

                    var assertmsg = $"---> {op}" + (inverseTerm ? " inversed" : "");

                    switch (op)
                    {
                        case Face.BooleanMode.Union:
                            {
                                Assert.True(res.Count == 2 && res[0] == firstTerm && res[1] == secondTerm);
                            }
                            break;

                        case Face.BooleanMode.Intersect:
                            {
                                Assert.True(res.Count == 0, assertmsg);
                            }
                            break;

                        case Face.BooleanMode.Difference:
                            {
                                Assert.True(res.Count == 1 && res[0] == firstTerm);
                            }
                            break;
                    }

                }

            }

        }

    }

}
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

            foreach (var op in new[]
            {
                //Face.BooleanMode.Intersect,
                Face.BooleanMode.Difference,
                // Face.BooleanMode.Union
            })
            {

                foreach (var inverseTerm in new[]
                {
                    // false,
                    true
                })
                {
                    DxfDocument? outdxf = null;
                    outdxf = new DxfDocument();

                    for (int layernum = 1; layernum <= 45; ++layernum)
                    {

                        // if (layernum != 9) continue;

                        DxfDocument? outdxf2 = null;
                        // outdxf2 = new DxfDocument();

                        var tol = 1e-8;

                        var layer = dxf.Layers.First(w => w.Name == $"layer {layernum}");

                        var faceGreen = dxf.LwPolylines.Where(w => w.Layer == layer && w.Color.Index == AciColor.Green.Index).ToFace(tol);
                        var faceYellow = dxf.LwPolylines.Where(w => w.Layer == layer && w.Color.Index == AciColor.Yellow.Index).ToFace(tol);
                        try
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

                            switch (op)
                            {
                                case Face.BooleanMode.Union:
                                    {
                                        switch (layernum)
                                        {
                                            case 1:
                                                // Assert.True(res.Count == 2 && res[0].Loops.Count == 2 && res[1].Loops.Count == 1, assertmsg);
                                                // res[0].Loops[0].Area.AssertEqualsTol(tol, 900, assertmsg);
                                                // res[0].Loops[1].Area.AssertEqualsTol(tol, 576, assertmsg);
                                                // res[1].Loops[0].Area.AssertEqualsTol(tol, 196, assertmsg);
                                                break;
                                        }
                                    }
                                    break;

                                case Face.BooleanMode.Intersect:
                                    {
                                        switch (layernum)
                                        {

                                        }
                                    }
                                    break;

                                case Face.BooleanMode.Difference:
                                    {
                                        if (!inverseTerm)
                                            switch (layernum)
                                            {

                                            }
                                        else
                                            switch (layernum)
                                            {

                                            }
                                    }
                                    break;
                            }

                            //Assert.True(res.Count == 0);

                            if (outdxf != null)
                            {
                                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                                outdxf.Viewport.ShowGrid = false;
                                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
                            }
                        }
                        catch (Exception ex)
                        {
                            outdxf?.AddEntities(faceGreen.Loops.Select(loop => { var ent = loop.DxfEntity(tol).Set(x => x.SetColor(AciColor.Red)); ent.Layer = new netDxf.Tables.Layer(layer.Name); return ent; }));
                            outdxf?.AddEntities(faceYellow.Loops.Select(loop => { var ent = loop.DxfEntity(tol).Set(x => x.SetColor(AciColor.Red)); ent.Layer = new netDxf.Tables.Layer(layer.Name); return ent; }));

                        }
                    }

;
                }

            }

        }

    }

}
namespace SearchAThing.Sci.Tests;

public partial class LoopTests
{

    [Fact]
    public void LoopTest_0018()
    {
        var dxf = netDxf.DxfDocument.Load(
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0018.dxf"));


        foreach (var op in new[] { Face.BooleanMode.Intersect, Face.BooleanMode.Difference, Face.BooleanMode.Union })
        {

            foreach (var inverseTerm in new[] { false, true })
            {

                DxfDocument? outdxf = null;
                // outdxf = new DxfDocument();

                DxfDocument? outdxf2 = null;
                // outdxf2 = new DxfDocument();

                var tol = 1e-8;

                var faceYellow = dxf.Entities.Polylines2D.Where(w => w.Layer.Name == "yellow").ToFace(tol);
                var faceGreen = dxf.Entities.Polylines2D.Where(w => w.Layer.Name == "green").ToFace(tol);

                outdxf?.AddEntities(faceGreen.Loops.Select(loop => loop.DxfEntity(tol).Act(x => x.SetColor(AciColor.Green))));
                outdxf?.AddEntities(faceYellow.Loops.Select(loop => loop.DxfEntity(tol).Act(x => x.SetColor(AciColor.Yellow))));

                var firstTerm = inverseTerm ? faceGreen : faceYellow;
                var secondTerm = inverseTerm ? faceYellow : faceGreen;

                var layerRes = new netDxf.Tables.Layer("res") { Color = AciColor.Cyan };

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

                switch (op)
                {
                    case Face.BooleanMode.Intersect:
                        {
                            Assert.True(res.Count == 4);
                            foreach (var x in res)
                            {
                                Assert.True(x.Loops.Count == 1);
                                x.Loops[0].Area.AssertEqualsTol(tol, 9);
                                x.Loops[0].Length.AssertEqualsTol(tol, 12.00000000);
                            }
                        }
                        break;

                    case Face.BooleanMode.Difference:
                        {
                            if (!inverseTerm)
                            {
                                Assert.True(res.Count == 4);

                                Assert.True(res[0].Loops.Count == 1);
                                res[0].Area.AssertEqualsTol(tol, 102);
                                res[0].Loops[0].Length.AssertEqualsTol(tol, 74.00000000);

                                Assert.True(res[1].Loops.Count == 1);
                                res[1].Area.AssertEqualsTol(tol, 102);
                                res[1].Loops[0].Length.AssertEqualsTol(tol, 74.00000000);

                                Assert.True(res[2].Loops.Count == 1);
                                res[2].Area.AssertEqualsTol(tol, 42);
                                res[2].Loops[0].Length.AssertEqualsTol(tol, 34.00000000);

                                Assert.True(res[3].Loops.Count == 1);
                                res[3].Area.AssertEqualsTol(tol, 42);
                                res[3].Loops[0].Length.AssertEqualsTol(tol, 34.00000000);
                            }
                            else
                            {
                                Assert.True(res.Count == 3);

                                Assert.True(res[0].Loops.Count == 1);
                                res[0].Area.AssertEqualsTol(tol, 101.99999999999986);
                                res[0].Loops[0].Length.AssertEqualsTol(tol, 73.99999999999996);

                                Assert.True(res[1].Loops.Count == 1);
                                res[1].Area.AssertEqualsTol(tol, 101.9999999999998);
                                res[1].Loops[0].Length.AssertEqualsTol(tol, 74.00000000000003);

                                Assert.True(res[2].Loops.Count == 2);
                                res[2].Loops[0].Area.AssertEqualsTol(tol, 168);
                                res[2].Loops[0].Length.AssertEqualsTol(tol, 84.00000000);
                                res[2].Loops[1].Area.AssertEqualsTol(tol, 28);
                                res[2].Loops[1].Length.AssertEqualsTol(tol, 32.00000000);
                            }
                        }
                        break;

                    case Face.BooleanMode.Union:
                        {
                            Assert.True(res.Count == 1 && res[0].Loops.Count == 8);

                            res[0].Loops[0].Area.AssertEqualsTol(tol, 1200);
                            res[0].Loops[0].Length.AssertEqualsTol(tol, 160.00000000);

                            var typeA_cnt = 0;
                            var typeB_cnt = 0;
                            var typeC_cnt = 0;

                            for (int i = 1; i < 8; ++i)
                            {
                                var loop = res[0].Loops[i];

                                if (loop.Area.EqualsTol(tol, 98) && loop.Length.EqualsTol(tol, 42.00000000))
                                    ++typeA_cnt;

                                if (loop.Area.EqualsTol(tol, 56) && loop.Length.EqualsTol(tol, 36.00000000))
                                    ++typeB_cnt;

                                if (loop.Area.EqualsTol(tol, 28) && loop.Length.EqualsTol(tol, 32.00000000))
                                    ++typeC_cnt;
                            }

                            Assert.True(typeA_cnt == 4 && typeB_cnt == 2 && typeC_cnt == 1);
                        }
                        break;
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
            }
        }
    }

}
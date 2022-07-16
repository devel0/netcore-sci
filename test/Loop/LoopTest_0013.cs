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
        public void LoopTest_0013()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0013.dxf"));

            DxfDocument? outdxf = null;
            outdxf = new DxfDocument();

            DxfDocument? outdxf2 = null;
            // outdxf2 = new DxfDocument();

            var tol = 1e-8;

            var faceGreen = dxf.LwPolylines.First(w => w.Color.Index == AciColor.Green.Index).ToLoop(tol).ToFace();
            var faceYellow = dxf.LwPolylines.First(w => w.Color.Index == AciColor.Yellow.Index).ToLoop(tol).ToFace();
            var faceMagenta = dxf.LwPolylines.First(w => w.Color.Index == AciColor.Magenta.Index).ToLoop(tol).ToFace();

            // outdxf?.AddEntity(loopGreen.DxfEntity(tol).Set(x => x.SetColor(AciColor.Green)));
            // outdxf?.AddEntity(loopYellow.DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow)));
            // outdxf?.AddEntity(loopMagenta.DxfEntity(tol).Set(x => x.SetColor(AciColor.Magenta)));

            var yellowIntersectMagenta = faceYellow.Boolean(tol, faceMagenta).ToList();
            var greenIntersectMagenta = faceGreen.Boolean(tol, faceMagenta).ToList();

            Assert.True(yellowIntersectMagenta.Count == 1 && yellowIntersectMagenta[0].Loops.Count == 1);
            yellowIntersectMagenta[0].Loops[0].Area.AssertEqualsTol(tol, 109.41172066);
            outdxf?.AddEntity(yellowIntersectMagenta[0].Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow)));

            Assert.True(greenIntersectMagenta.Count == 1 && greenIntersectMagenta[0].Loops.Count == 1);
            greenIntersectMagenta[0].Loops[0].Area.AssertEqualsTol(tol, 88.60501699);
            outdxf?.AddEntity(greenIntersectMagenta[0].Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Green)));

            var yellowIntSubGreenInt = yellowIntersectMagenta[0]
                .Boolean(tol, greenIntersectMagenta[0], Face.BooleanMode.Difference, outdxf2).ToList();

            Assert.True(yellowIntSubGreenInt.Count == 3 && yellowIntSubGreenInt.All(w => w.Loops.Count == 1));
            foreach (var res in yellowIntSubGreenInt)
            {
                outdxf?.AddEntity(res.Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Red)));
                outdxf?.AddEntity(res
                    .Loops[0]
                    .ToHatch(tol, new HatchPattern(HatchPattern.Line.Name) { Angle = 45, Scale = 0.5 })
                    .SetColor(AciColor.Red));
            }

            yellowIntSubGreenInt[0].Loops[0].Area.AssertEqualsTol(tol, 5.71051935);
            yellowIntSubGreenInt[0].Loops[0].Length.AssertEqualsTol(tol, 13.51031587);

            yellowIntSubGreenInt[1].Loops[0].Area.AssertEqualsTol(tol, 8.2284423);
            yellowIntSubGreenInt[1].Loops[0].Length.AssertEqualsTol(tol, 18.68742254);

            yellowIntSubGreenInt[2].Loops[0].Area.AssertEqualsTol(tol, 6.86774201);
            yellowIntSubGreenInt[2].Loops[0].Length.AssertEqualsTol(tol, 15.85467970);

            {
                var q = yellowIntSubGreenInt[1].Loops[0].ToLwPolyline(tol);
                q.Vertexes[0].Bulge.AssertEqualsTol(1e-14, -0.017033061012458925);
                q.Vertexes[0].Position.X.AssertEqualsTol(1e-14, 14);
                q.Vertexes[0].Position.Y.AssertEqualsTol(1e-14, 11.178243024937577);
            }



            // lwpolyline unit test
            //yellowIntSubGreenInt[1].ToLwPolyline(tol).Vertexes[0].Bulge.AssertEqualsTol(1e-5, 0);

            //var gyInts = loopGreen.Intersect(tol, loopYellow).ToList();

            // Assert.True(yellowIntSubGreenInt.Count == 1);
            // yellowIntSubGreenInt[0].Area.AssertEqualsTol(tol, 88.60501699);
            // yellowIntSubGreenInt[0].Length.AssertEqualsTol(tol, 48.75624125);

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
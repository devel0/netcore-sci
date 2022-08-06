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
        public void LoopTest_0011()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0011.dxf"));

            DxfDocument? outdxf = null;
            // outdxf = new DxfDocument();

            DxfDocument? outdxf2 = null;
            // outdxf2 = new DxfDocument();

            var tol = 1e-8;

            var faceGreen = dxf.Entities.Polylines2D.First(w => w.Color.Index == AciColor.Green.Index).ToLoop(tol).ToFace();
            var faceYellow = dxf.Entities.Polylines2D.First(w => w.Color.Index == AciColor.Yellow.Index).ToLoop(tol).ToFace();
            var faceMagenta = dxf.Entities.Polylines2D.First(w => w.Color.Index == AciColor.Magenta.Index).ToLoop(tol).ToFace();

            outdxf?.AddEntity(faceGreen.Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Green)));
            outdxf?.AddEntity(faceYellow.Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow)));
            outdxf?.AddEntity(faceMagenta.Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Magenta)));

            var yellowIntersectMagenta = faceYellow.Boolean(tol, faceMagenta).ToList();
            var greenIntersectMagenta = faceGreen.Boolean(tol, faceMagenta).ToList();

            Assert.True(yellowIntersectMagenta.Count == 1 && yellowIntersectMagenta[0].Loops.Count == 1);
            yellowIntersectMagenta[0].Loops[0].Area.AssertEqualsTol(tol, 109.41172066);
            outdxf?.AddEntity(yellowIntersectMagenta[0].Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow)));

            Assert.True(greenIntersectMagenta.Count == 1 && greenIntersectMagenta[0].Loops.Count == 1);
            greenIntersectMagenta[0].Loops[0].Area.AssertEqualsTol(tol, 88.60501699);
            outdxf?.AddEntity(greenIntersectMagenta[0].Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Green)));

            var A = yellowIntersectMagenta[0];
            var B = greenIntersectMagenta[0];

            var yellowIntSubGreenInt = A.Boolean(tol, B, Face.BooleanMode.Intersect, outdxf2).ToList();
            foreach (var res in yellowIntSubGreenInt)
            {
                outdxf?.AddEntity(res.Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Red)));
            }
            ;
            //var gyInts = loopGreen.Intersect(tol, loopYellow).ToList();

            Assert.True(yellowIntSubGreenInt.Count == 1 && yellowIntSubGreenInt[0].Loops.Count == 1);
            yellowIntSubGreenInt[0].Loops[0].Area.AssertEqualsTol(tol, 88.60501699);
            yellowIntSubGreenInt[0].Loops[0].Length.AssertEqualsTol(tol, 48.75624125);

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
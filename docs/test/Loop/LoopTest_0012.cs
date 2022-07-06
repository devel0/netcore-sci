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
        public void LoopTest_0012()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0012.dxf"));

            DxfDocument? outdxf = null;
            //outdxf = new DxfDocument();

            var tol = 1e-8;

            var loopGreen = dxf.LwPolylines.First(w => w.Color.Index == AciColor.Green.Index).ToLoop(tol);
            var loopYellow = dxf.LwPolylines.First(w => w.Color.Index == AciColor.Yellow.Index).ToLoop(tol);
            var loopBlue = dxf.LwPolylines.First(w => w.Color.Index == AciColor.Blue.Index).ToLoop(tol).InvertSense(tol);

            outdxf?.AddEntity(loopGreen.DxfEntity(tol).Set(x => x.SetColor(AciColor.Green)));
            outdxf?.AddEntity(loopYellow.DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow)));

            var yellowIntersectGreen = loopYellow.Boolean(tol, loopGreen).ToList();
            var blueIntersectGreen = loopBlue.Boolean(tol, loopGreen).ToList();

            Assert.True(yellowIntersectGreen.Count == 1);
            yellowIntersectGreen[0].Area.AssertEqualsTol(tol, 160);
            yellowIntersectGreen[0].Length.AssertEqualsTol(tol, 52.00000000);

            outdxf?.AddEntity(yellowIntersectGreen[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Red)));

            Assert.True(blueIntersectGreen.Count == 1);
            blueIntersectGreen[0].Area.AssertEqualsTol(tol, 160);
            blueIntersectGreen[0].Length.AssertEqualsTol(tol, 52.00000000);

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }

    }

}
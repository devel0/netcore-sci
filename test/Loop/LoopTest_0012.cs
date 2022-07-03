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
            outdxf = new DxfDocument();

            var tol = 1e-8;

            var loopGreen = dxf.LwPolylines.First(w => w.Color.Index == AciColor.Green.Index).ToLoop(tol);
            var loopYellow = dxf.LwPolylines.First(w => w.Color.Index == AciColor.Yellow.Index).ToLoop(tol);

            var loopGreenFromJson = JsonConvert.DeserializeObject<Loop>(
                File.ReadAllText("Loop/LoopTest_0012-loop1.json"), SciToolkit.SciJsonSettings);

            var loopYellowFromJson = JsonConvert.DeserializeObject<Loop>(
                File.ReadAllText("Loop/LoopTest_0012-loop2.json"), SciToolkit.SciJsonSettings);

            // loopGreen = loopGreenFromJson;
            // loopYellow = loopYellowFromJson;

            outdxf?.AddEntity(loopGreen.DxfEntity(tol).Set(x => x.SetColor(AciColor.Green)));
            outdxf?.AddEntity(loopYellow.DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow)));

            var yellowIntersectGreen = loopYellow.Intersect(tol, loopGreen, outdxf).ToList();

            Assert.True(yellowIntersectGreen.Count == 1);
            yellowIntersectGreen[0].Area.AssertEqualsTol(tol, 88.60501699);
            yellowIntersectGreen[0].Length.AssertEqualsTol(tol, 48.75624125);

            var greenIntersectYellow = loopGreen.Intersect(tol, loopYellow, outdxf).ToList();

            Assert.True(greenIntersectYellow.Count == 1);
            greenIntersectYellow[0].Area.AssertEqualsTol(tol, 88.60501699);
            greenIntersectYellow[0].Length.AssertEqualsTol(tol, 48.75624125);

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }

    }

}
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
        public void LoopTest_0010()
        {
            DxfDocument? outdxf = null;
            //outdxf = new DxfDocument();

            var tol = 0.1;//1e-8;

            var loop1 = JsonConvert.DeserializeObject<Loop>(
                File.ReadAllText("Loop/LoopTest_0010-loop1.json"), SciToolkit.SciJsonSettings);

            var loop2 = JsonConvert.DeserializeObject<Loop>(
                File.ReadAllText("Loop/LoopTest_0010-loop2.json"), SciToolkit.SciJsonSettings);

            var loopYellow = loop1;
            var loopGreen = loop2;

            var ints = loopYellow.Intersect(tol, loopGreen).ToList();

            outdxf?.AddEntity(loopGreen.DxfEntity(tol).Set(x => x.SetColor(AciColor.Green)));
            outdxf?.AddEntity(loopYellow.DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow)));

            Assert.True(ints.Count == 0);
            //outdxf?.AddEntity(ints[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Red))); 

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }

    }

}
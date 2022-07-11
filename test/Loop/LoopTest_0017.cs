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
        public void LoopTest_0017()
        {
            DxfDocument? outdxf = null;
            // outdxf = new DxfDocument();

            DxfDocument? outdxf2 = null;
            // outdxf2 = new DxfDocument();

            var tol = 1e-8;

            var loop1 = JsonConvert.DeserializeObject<Loop>(
                File.ReadAllText("Loop/LoopTest_0017-loop1.json"), SciToolkit.SciJsonSettings);

            var loop2 = JsonConvert.DeserializeObject<Loop>(
                File.ReadAllText("Loop/LoopTest_0017-loop2.json"), SciToolkit.SciJsonSettings);

            var loopYellow = loop1;
            var loopGreen = loop2;
            // Difference
            var ints = loopYellow.Boolean(tol, loopGreen, Loop.BooleanMode.Intersect, outdxf2).ToList();

            outdxf?.AddEntity(loopGreen.DxfEntity(tol).Set(x => x.SetColor(AciColor.Green)));
            outdxf?.AddEntity(loopYellow.DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow)));
            outdxf?.AddEntity(ints[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Red)));

            Assert.True(ints.Count == 1);
            ints[0].Area.AssertEqualsTol(tol, 16.68099798);
            ints[0].Length.AssertEqualsTol(tol, 21.16635090);            

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
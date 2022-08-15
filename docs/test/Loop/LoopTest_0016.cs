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
        public void LoopTest_0016()
        {
            DxfDocument? outdxf = null;
            // outdxf = new DxfDocument();

            DxfDocument? outdxf2 = null;
            // outdxf2 = new DxfDocument();

            var tol = 1e-8;

            var face1 = JsonConvert.DeserializeObject<Loop>(
                File.ReadAllText("Loop/LoopTest_0016-loop1.json"), SciToolkit.SciJsonSettings)!.ToFace();

            var face2 = JsonConvert.DeserializeObject<Loop>(
                File.ReadAllText("Loop/LoopTest_0016-loop2.json"), SciToolkit.SciJsonSettings)!.ToFace();

            var faceYellow = face1;
            var faceGreen = face2;

            var ints = faceYellow.Boolean(tol, faceGreen, Face.BooleanMode.Difference, outdxf2).ToList();

            outdxf?.AddEntity(faceGreen.Loops[0].DxfEntity(tol).Act(x => x.SetColor(AciColor.Green)));
            outdxf?.AddEntity(faceYellow.Loops[0].DxfEntity(tol).Act(x => x.SetColor(AciColor.Yellow)));

            Assert.True(ints.Count == 1);
            ints[0].Loops[0].Area.AssertEqualsTol(tol, 4.75);
            ints[0].Loops[0].Length.AssertEqualsTol(tol, 20.00000000);

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
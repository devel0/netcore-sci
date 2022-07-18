using Xunit;
using System.Linq;
using System;
using netDxf;
using netDxf.Entities;
using Newtonsoft.Json;
using System.IO;

namespace SearchAThing.Sci.Tests
{
    public partial class LoopTests
    {

        [Fact]
        public void LoopTest_0008()
        {
            DxfDocument? outdxf = null;
            //outdxf = new DxfDocument();

            var tol = 0.1;//1e-8;

            var faceYellow = JsonConvert.DeserializeObject<Loop>(
                File.ReadAllText("Loop/LoopTest_0008-loop1.json"), SciToolkit.SciJsonSettings)!.ToFace();

            var faceGreen = JsonConvert.DeserializeObject<Loop>(
                File.ReadAllText("Loop/LoopTest_0008-loop2.json"), SciToolkit.SciJsonSettings)!.ToFace();

            var bboxYellow = faceYellow.Loops[0].Vertexes(tol).BBox();
            var bboxGreen = faceGreen.Loops[0].Vertexes(tol).BBox();

            var ints = faceYellow.Boolean(tol, faceGreen).ToList();

            outdxf?.AddEntity(faceGreen.Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Green)));
            outdxf?.AddEntity(faceYellow.Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow)));

            Assert.True(ints.Count == 1);
            outdxf?.AddEntity(ints[0].Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Red)));

            ints[0].Loops[0].Area.AssertEqualsTol(tol, 33.0552);
            ints[0].Loops[0].Length.AssertEqualsTol(tol, 51.4801);

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }

    }

}
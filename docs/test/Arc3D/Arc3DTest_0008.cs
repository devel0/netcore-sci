using Xunit;
using System.Linq;
using System;
using static System.Math;
using netDxf;
using Newtonsoft.Json;
using System.IO;

using static SearchAThing.SciToolkit;

namespace SearchAThing.Sci.Tests
{
    public partial class Arc3DTests
    {

        [Fact]
        public void Arc3DTest_0008()
        {
            DxfDocument? outdxf = null;
            //outdxf = new DxfDocument();

            var tol = 1e-3;

            var p1 = new Vector3D(0, 12.72);
            var p2 = new Vector3D(1.90117000, 17.09880000);
            var p3 = new Vector3D(6.28, 19);

            var arc = new Arc3D(tol, p1, p2, p3);
            outdxf?.AddEntity(arc.DxfEntity);
            outdxf?.AddEntity(p1.DxfEntity);
            outdxf?.AddEntity(p2.DxfEntity);
            outdxf?.AddEntity(p3.DxfEntity);

            var arcj = JsonConvert.DeserializeObject<Arc3D>(
                File.ReadAllText("Arc3D/Arc3DTest_0008.json"), SciToolkit.SciJsonSettings)
                .Act(w => Assert.NotNull(w))!;
            var line1 = new Line3D(new Vector3D(0, 20), p1);
            var line2 = new Line3D(p3, line1.From);
            var geoms = new Edge[] { line1, arcj, line2 };
            var loop = new Loop(tol, geoms, Plane3D.XY);
            outdxf?.AddEntity(arcj.DxfEntity.Act(x => x.SetColor(AciColor.Cyan)));

            var lw = loop.DxfEntity(tol).Act(x => x.SetColor(AciColor.Green));
            Assert.True(lw.Vertexes.Count == 3);
            lw.Vertexes[1].Bulge.AssertEqualsTol(TwoPIRadTol, -0.39453055322534736);
            outdxf?.AddEntity(lw);

            // arc.Length.AssertEqualsTol(1e-8, 0.44008396);
            // arc.CircularSectorArea.AssertEqualsTol(1e-8, 1.43109185);

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }
    }

}
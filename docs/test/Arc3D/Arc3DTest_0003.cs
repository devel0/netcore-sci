using Xunit;
using System.Linq;
using System;
using static System.Math;
using netDxf;

namespace SearchAThing.Sci.Tests
{
    public partial class Arc3DTests
    {

        [Fact]
        public void Arc3DTest_0003()
        {
            DxfDocument? outdxf = null;
            //outdxf = new DxfDocument();

            var tolBuildArc = 1e-1;
            var tol = 1e-7;            

            var arcFrom = new Vector3D(0.000000000000003, 12.72, 0);
            var arcMiddle = new Vector3D(1.90117406287241, 17.0988259371276, 0);
            var arcTo = new Vector3D(6.28, 19, 0);

            var arc = new Arc3D(
                tolBuildArc,
                fromPt: arcFrom,
                insidePt: arcMiddle,
                toPt: arcTo);

            // arc.From.AssertEqualsTol(tol, arcFrom);
            // arc.MidPoint.AssertEqualsTol(tol, arcMiddle);
            // arc.To.AssertEqualsTol(tol, arcTo);

            outdxf?.AddEntity(arc.DxfEntity);

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }
    }

}
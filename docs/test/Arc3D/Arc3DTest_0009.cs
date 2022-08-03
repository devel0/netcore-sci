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
        public void Arc3DTest_0009()
        {
            DxfDocument? outdxf = null;
            // outdxf = new DxfDocument();

            var tol = 1e-1;

            var p1 = new Vector3D(238, 24, 0);
            var p2 = new Vector3D(234.48528137423858, 15.514718625761429, 0);
            var p3 = new Vector3D(226, 12, 0);

            var arc = new Arc3D(tol, p1, p2, p3);
            outdxf?.AddEntity(arc.DxfEntity);
            outdxf?.AddEntity(p1.DxfEntity);
            outdxf?.AddEntity(p2.DxfEntity);
            outdxf?.AddEntity(p3.DxfEntity);

            arc.AngleStart.AssertEqualsTol(TwoPIRadTol, 0);
            arc.AngleEnd.AssertEqualsTol(TwoPIRadTol, 1.5707963267948966);
            arc.CS.BaseZ.AssertEqualsTol(NormalizedLengthTolerance, 0, 0, -1);

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }
    }

}
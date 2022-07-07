using Xunit;
using System.Linq;
using System;
using static System.Math;
using Newtonsoft.Json;
using System.IO;

using static SearchAThing.SciToolkit;
using netDxf;

namespace SearchAThing.Sci.Tests
{
    public partial class Arc3DTests
    {

        [Fact]
        public void Arc3DTest_0005()
        {
            var tol = 1e-8;

            DxfDocument? outdxf = null;
            //outdxf = new DxfDocument();

            var p1 = new Vector3D(0, 12.72);
            var p2 = new Vector3D(1.90117406, 17.09882594);
            var p3 = new Vector3D(6.28000000, 19.00000000);

            var arcA = new Arc3D(tol, p1, p2, p3);
            var arcB = new Arc3D(tol, p1, p2, p3, normal: -arcA.CS.BaseZ);            

            arcA.GeomFrom.AssertEqualsTol(tol, arcB.GeomTo);
            arcA.GeomTo.AssertEqualsTol(tol, arcB.GeomFrom);            

            outdxf?.AddEntity(arcA.DxfEntity);
            outdxf?.AddEntity(arcB.DxfEntity);

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }

    }

}
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
            // outdxf = new DxfDocument();

            var p1 = new Vector3D(0, 12.72);
            var p2 = new Vector3D(1.90117406, 17.09882594);
            var p3 = new Vector3D(6.28000000, 19.00000000);

            var arcRed = new Arc3D(tol, p1, p2, p3);
            var arcGreen = new Arc3D(tol, p3, p2, p1);

            arcRed.GeomFrom.AssertEqualsTol(tol, arcGreen.GeomTo);
            arcRed.GeomTo.AssertEqualsTol(tol, arcGreen.GeomFrom);

            arcRed.SGeomFrom.AssertEqualsTol(tol, arcGreen.SGeomTo);
            arcRed.SGeomTo.AssertEqualsTol(tol, arcGreen.SGeomFrom);

            arcRed.CS.BaseZ.AssertEqualsTol(NormalizedLengthTolerance, -arcGreen.CS.BaseZ);

            outdxf?.AddEntity(arcRed.DxfEntity.Act(ent => ent.Color = AciColor.Red));
            outdxf?.AddEntity(arcGreen.DxfEntity.Act(ent => ent.Color = AciColor.Green));

            var arcGreen2 = arcGreen.ToggleSense();
            arcRed.GeomFrom.AssertEqualsTol(tol, arcGreen2.GeomTo);
            arcRed.GeomTo.AssertEqualsTol(tol, arcGreen2.GeomFrom);

            arcRed.SGeomFrom.AssertEqualsTol(tol, arcGreen2.SGeomFrom);
            arcRed.SGeomTo.AssertEqualsTol(tol, arcGreen2.SGeomTo);

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }

    }

}
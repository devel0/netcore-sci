using Xunit;
using System.Linq;
using System;
using System.Collections.Generic;
using netDxf;

namespace SearchAThing.Sci.Tests
{
    public partial class PolygonTests
    {

        // TODO: not yet finished

        [Fact]
        public void PolygonTest_0007()
        {
            DxfDocument? outdxf = null;
            outdxf = new DxfDocument();

            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Polygon/PolygonTest_0007.dxf"));

            var tol = 1e-8;

            var poly = dxf.Entities.Polylines2D.First(w => w.Color.Index == AciColor.Cyan.Index);
            outdxf?.AddEntity((netDxf.Entities.Polyline2D)poly.Clone());

            var refpred = new Vector3D(34.3, 32.9);
            outdxf?.AddEntity(refpred.DxfEntity.Set(w => w.Color = AciColor.Red));
            // var polyred = poly.Offset(tol, refpred, 0.1);
            // outdxf?.AddEntity(polyred.Set(x => x.Color = AciColor.Red));

            var polyred = poly.OffsetGeoms(tol, refpred, 0.1);
            outdxf?.AddEntities(polyred.Select(x => x.DxfEntity.Set(x => x.Color = AciColor.Red)));

            //var refp2 = new Vector3D(34.1000, 33.2000);            

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }

    }
}
using Xunit;
using System.Linq;
using System;
using System.Collections.Generic;
using netDxf;

namespace SearchAThing.Sci.Tests
{
    public partial class PolygonTests
    {

        [Fact]
        public void PolygonTest_0007()
        {
            DxfDocument? outdxf = null;
            // outdxf = new DxfDocument();

            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Polygon/PolygonTest_0007.dxf"));

            var tol = 1e-8;

            var poly = dxf.Entities.Polylines2D.First(w => w.Color.Index == AciColor.Cyan.Index);
            var refpyellow = dxf.Entities.Points.First(w => w.Color.Index == AciColor.Yellow.Index);
            var refpgreen = dxf.Entities.Points.First(w => w.Color.Index == AciColor.Green.Index);

            outdxf?.AddEntity((netDxf.Entities.Polyline2D)poly.Clone());
            outdxf?.AddEntity((netDxf.Entities.Point)refpyellow.Clone());
            outdxf?.AddEntity((netDxf.Entities.Point)refpgreen.Clone());

            // var polyred = poly.Offset(tol, refpred, 0.1);
            // outdxf?.AddEntity(polyred.Set(x => x.Color = AciColor.Red));            

            var poly_cs = poly.CS();

            var polyyellow = poly
                .OffsetGeoms(tol, refpyellow.Position, 0.1)
                .AutoTrimExtends(tol)
                .ToLwPolyline(tol, poly_cs, closed: poly.IsClosed);
            outdxf?.AddEntity(polyyellow.Act(x => x.Color = AciColor.Yellow));
            // outdxf?.AddEntities(polyyellow.Select(x=>x.DxfEntity.Set(x => x.Color = AciColor.Yellow)));

            var loopyellow = polyyellow.ToLoop(tol);
            loopyellow.Area.AssertEqualsTol(tol, 3.97251611);
            loopyellow.Length.AssertEqualsTol(tol, 27.19882398);

            //

            var polygreen = poly
                .OffsetGeoms(tol, refpgreen.Position, 0.1)
                .AutoTrimExtends(tol)
                .ToLwPolyline(tol, poly_cs, closed: poly.IsClosed);
            outdxf?.AddEntity(polygreen.Act(x => x.Color = AciColor.Green));
            // outdxf?.AddEntities(polygreen.Select(x=>x.DxfEntity.Set(x => x.Color = AciColor.Green)));

            var loopgreen = polygreen.ToLoop(tol);
            loopgreen.Area.AssertEqualsTol(tol, 9.60640941);
            loopgreen.Length.AssertEqualsTol(tol, 29.15050266);

            //

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }

    }
}
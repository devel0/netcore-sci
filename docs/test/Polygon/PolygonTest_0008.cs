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
        public void PolygonTest_0008()
        {
            DxfDocument? outdxf = null;
            // outdxf = new DxfDocument();

            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Polygon/PolygonTest_0007.dxf"));

            var tol = 1e-8;

            var poly = dxf.Entities.Polylines2D.First(w => w.Color.Index == AciColor.Cyan.Index);
            var poly_exploded = poly.ToGeometries(tol);

            var arcs = poly_exploded.OfType<Arc3D>().ToList();

            //            

            Assert.False(arcs[0].Sense); // reversed

            var aStartDeg = 210.84103981;
            var aEndDeg = 214.7237306;
            var pStart = new Vector3D(35.45961505, 23.5, 0);
            var pEnd = new Vector3D(35.78162554, 23, 0);

            void AssertData(Arc3D arc)
            {
                Assert.True(arc.CS.Equals(tol, CoordinateSystem3D.WCS.Move(arc.Center)));

                arc.AngleStartDeg.AssertEqualsTol(tol, aStartDeg);
                arc.AngleEndDeg.AssertEqualsTol(tol, aEndDeg);
                arc.GeomFrom.AssertEqualsTol(tol, pStart);
                arc.GeomTo.AssertEqualsTol(tol, pEnd);

                if (arc.Sense)
                {
                    arc.SensedAngleStartDeg.AssertEqualsTol(tol, aStartDeg);
                    arc.SensedAngleEndDeg.AssertEqualsTol(tol, aEndDeg);
                    arc.SGeomFrom.AssertEqualsTol(tol, pStart);
                    arc.SGeomTo.AssertEqualsTol(tol, pEnd);
                }
                else
                {
                    arc.SensedAngleStartDeg.AssertEqualsTol(tol, aEndDeg);
                    arc.SensedAngleEndDeg.AssertEqualsTol(tol, aStartDeg);
                    arc.SGeomFrom.AssertEqualsTol(tol, pEnd);
                    arc.SGeomTo.AssertEqualsTol(tol, pStart);
                }

            }
            AssertData(arcs[0]);

            //

            Assert.False(arcs[1].Sense); // reversed

            aStartDeg = 147.06932466;
            aEndDeg = 212.93067534;
            pStart = new Vector3D(34.04837448, 32.5, 0);
            pEnd = new Vector3D(34.04837448, 23.5, 0);
            AssertData(arcs[1]);

            //

            Assert.False(arcs[2].Sense); // reversed

            aStartDeg = 145.2762694;
            aEndDeg = 149.15896019;
            pStart = new Vector3D(35.78162554, 33, 0);
            pEnd = new Vector3D(35.45961505, 32.5);
            AssertData(arcs[2]);

            //

            Assert.True(arcs[3].Sense); // not reversed

            aStartDeg = 145.2762694;
            aEndDeg = 214.7237306;
            pStart = new Vector3D(33.78162554, 33, 0);
            pEnd = new Vector3D(33.78162554, 23, 0);
            AssertData(arcs[3]);

            //

            outdxf?.AddEntities(poly_exploded.Select(w => w.DxfEntity));

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }
        }

    }
}
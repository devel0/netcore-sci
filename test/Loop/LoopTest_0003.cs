using Xunit;
using System.Linq;
using System;
using netDxf;
using netDxf.Entities;

namespace SearchAThing.Sci.Tests
{
    public partial class PolygonTests
    {

        [Fact]
        public void LoopTest_0003()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0003.dxf"));

            DxfDocument? outdxf = null;
            outdxf = new DxfDocument();

            var tol = 1e-8;

            var loopGreen = dxf.LwPolylines.First(w => w.Layer.Name == "green").ToLoop(tol);
            var loopYellow = dxf.LwPolylines.First(w => w.Layer.Name == "yellow").ToLoop(tol);

            var gyInts = loopGreen.Intersect(tol, loopYellow).ToList();

            Assert.True(gyInts.Count == 1);

            if (outdxf != null)
            {
                outdxf.AddEntity(loopGreen.DxfEntity(tol).Set(w => w.Layer = dxf.Layers.First(w => w.Name == "green")));
                outdxf.AddEntity(loopYellow.DxfEntity(tol).Set(w => w.Layer = dxf.Layers.First(w => w.Name == "yellow")));

                foreach (var gyInt in gyInts)
                {
                    outdxf.AddEntity(gyInt
                        .ToHatch(tol, new HatchPattern(HatchPattern.Line.Name) { Angle = 45, Scale = 0.5 })
                        .SetColor(AciColor.Cyan));
                }
            }

            loopYellow.Area.AssertEqualsTol(tol, gyInts[0].Area);
            gyInts[0].Length.AssertEqualsTol(tol, 47.89069988);

            {
                var delta = new Vector3D(1, 2, 3);
                var movedTest = loopYellow.Move(delta);

                for (int i = 0; i < loopYellow.Edges.Count; ++i)
                {
                    (loopYellow.Edges[i].SGeomFrom + delta).AssertEqualsTol(tol, movedTest.Edges[i].SGeomFrom);
                    (loopYellow.Edges[i].SGeomTo + delta).AssertEqualsTol(tol, movedTest.Edges[i].SGeomTo);
                }
            }

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }

        }

    }

}
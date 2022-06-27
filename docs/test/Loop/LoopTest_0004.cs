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
        public void LoopTest_0004()
        {
            DxfDocument? outdxf = null;
            outdxf = new DxfDocument();

            var tol = 1e-1;

            var loopGreen = new Loop(tol, new[]
            {
                new Line3D(1,1,0, 1,1,10),
                new Line3D(1,1,10, 6,1,10),
                new Line3D(6,1,10, 6,1,0),
                new Line3D(6,1,0, 1,1,0)
            });
            var loopYellow = new Loop(tol, new[]
            {
                new Line3D(1,1,0, 1,1,10),
                new Line3D(1,1,10, 4,1,10),
                new Line3D(4,1,10, 4,1,0),
                new Line3D(4,1,0, 1,1,0)
            });

            var gyInts = loopGreen.Intersect(tol, loopYellow).ToList();

            Assert.True(gyInts.Count == 1);            

            if (outdxf != null)
            {
                outdxf.AddEntity(loopGreen.DxfEntity(tol).Set(w => w.SetColor(AciColor.Green)));
                outdxf.AddEntity(loopYellow.DxfEntity(tol).Set(w => w.SetColor(AciColor.Yellow)));

                foreach (var gyInt in gyInts)
                {
                    outdxf.AddEntity(gyInt
                        .ToHatch(tol, new HatchPattern(HatchPattern.Line.Name) { Angle = 45, Scale = 0.5 })
                        .SetColor(AciColor.Cyan));
                }
            }

            gyInts[0].Area.AssertEqualsTol(tol, 30); 

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }

        }

    }

}
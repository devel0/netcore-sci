using Xunit;
using System.Linq;
using System;
using netDxf;

namespace SearchAThing.Sci.Tests
{
    public partial class PolygonTests
    {

        [Fact]
        public void LoopTest_0002()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0002.dxf"));

            var tol = 1e-8;

            var loopGreen = dxf.LwPolylines.First(w => w.Layer.Name == "green").ToLoop(tol);
            var loopRed = dxf.LwPolylines.First(w => w.Layer.Name == "red").ToLoop(tol);
            var loopMagenta = dxf.LwPolylines.First(w => w.Layer.Name == "magenta").ToLoop(tol);
            var loopCyan = dxf.LwPolylines.First(w => w.Layer.Name == "cyan").ToLoop(tol);
            var loopBlue = dxf.LwPolylines.First(w => w.Layer.Name == "blue").ToLoop(tol);
            var loopYellow = dxf.LwPolylines.First(w => w.Layer.Name == "yellow").ToLoop(tol);

            // check green fully contained into magenta
            {
                var q = loopGreen.Intersect(tol, loopMagenta).ToList();
                Assert.True(q.Count == 1);
                var ecnt = q[0].Edges.Count;
                Assert.True(ecnt == loopGreen.Edges.Count);
                for (int i = 0; i < ecnt; ++i) Assert.True(loopGreen.Edges[i] == q[0].Edges[i]);
            }

            // check red fully contained into green
            {
                var q = loopGreen.Intersect(tol, loopRed).ToList();
                Assert.True(q.Count == 1);
                var ecnt = q[0].Edges.Count;
                Assert.True(ecnt == loopRed.Edges.Count);
                for (int i = 0; i < ecnt; ++i) Assert.True(loopRed.Edges[i] == q[0].Edges[i]);
            }

            // blue not intersect green
            Assert.True(loopBlue.Intersect(tol, loopGreen).Count() == 0);

            {
                var loops = loopCyan.Intersect(tol, loopGreen).ToList();

                Assert.True(loops.Count == 2);
                Assert.True(loops[0].Area.EqualsTol(tol, 56.42492663));
                Assert.True(loops[1].Area.EqualsTol(tol, 360.04194237));

                //

                loops = loopGreen.Intersect(tol, loopCyan).ToList();

                Assert.True(loops.Count == 2);
                Assert.True(loops[0].Area.EqualsTol(tol, 56.42492663));
                Assert.True(loops[1].Area.EqualsTol(tol, 360.04194237));
            }

            {
                var loops = loopGreen.Intersect(tol, loopYellow).ToList();

                Assert.True(loops.Count == 1);
                Assert.True(loops[0].Area.EqualsTol(tol, 563.78939052));

                //

                // var loops = loopYellow.Intersect(tol, loopGreen).ToList();

                // Assert.True(loops.Count == 1);
                // Assert.True(loops[0].Area.EqualsTol(tol, 563.78939052));
            }

            // outtmp.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
            // outtmp.Viewport.ShowGrid = false;
            // outtmp.Save("/home/devel0/Desktop/out.dxf");

        }

    }
}
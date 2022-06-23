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

            var loopGreen = dxf.LwPolylines.First(w => w.Color.Index == netDxf.AciColor.Green.Index).ToLoop(tol);
            var loopRed = dxf.LwPolylines.First(w => w.Color.Index == netDxf.AciColor.Red.Index).ToLoop(tol);
            var loopMagenta = dxf.LwPolylines.First(w => w.Color.Index == netDxf.AciColor.Magenta.Index).ToLoop(tol);
            var loopCyan = dxf.LwPolylines.First(w => w.Color.Index == netDxf.AciColor.Cyan.Index).ToLoop(tol);
            var loopBlue = dxf.LwPolylines.First(w => w.Color.Index == netDxf.AciColor.Blue.Index).ToLoop(tol);

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

                var outtmp = new DxfDocument();
                foreach (var loop in loops)
                {
                    var lw = loop.ToLwPolyline(tol);
                    outtmp.AddEntity(lw);
                }

                // outtmp.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                // outtmp.Viewport.ShowGrid = false;
                // outtmp.Save("/home/devel0/Desktop/out.dxf");

                Assert.True(loops[0].Area.EqualsTol(tol, 56.42492663));
                Assert.True(loops[1].Area.EqualsTol(tol, 360.04194237));                
            }

        }

    }
}
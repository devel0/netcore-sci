using Xunit;
using System.Linq;
using System;
using netDxf;
using netDxf.Entities;
using System.Collections.Generic;

namespace SearchAThing.Sci.Tests
{
    public partial class LoopTests
    {

        [Fact]
        public void LoopTest_0002()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0002.dxf"));

            DxfDocument? outdxf = null;
            outdxf = new DxfDocument();

            var tol = 1e-8;

            var loopGreen = dxf.LwPolylines.First(w => w.Layer.Name == "green").ToLoop(tol);
            var loopRed = dxf.LwPolylines.First(w => w.Layer.Name == "red").ToLoop(tol);
            var loopMagenta = dxf.LwPolylines.First(w => w.Layer.Name == "magenta").ToLoop(tol);
            var loopCyan = dxf.LwPolylines.First(w => w.Layer.Name == "cyan").ToLoop(tol);
            var loopBlue = dxf.LwPolylines.First(w => w.Layer.Name == "blue").ToLoop(tol);
            var loopYellow = dxf.LwPolylines.First(w => w.Layer.Name == "yellow").ToLoop(tol);

            var faceGreen = new Face(loopGreen.Plane, new[] { loopGreen });
            var faceRed = new Face(loopRed.Plane, new[] { loopRed });
            var faceMagenta = new Face(loopMagenta.Plane, new[] { loopMagenta });
            var faceCyan = new Face(loopCyan.Plane, new[] { loopCyan });
            var faceBlue = new Face(loopBlue.Plane, new[] { loopBlue });
            var faceYellow = new Face(loopYellow.Plane, new[] { loopYellow });

            void dumpLoops(IEnumerable<Loop> loops, netDxf.Tables.Layer? layer = null)
            {
                if (outdxf != null)
                {
                    foreach (var loop in loops)
                    {
                        var hatch = loop.ToHatch(tol, new HatchPattern(HatchPattern.Line.Name)
                        {
                            Angle = 45,
                            Scale = 0.2
                        });
                        if (layer != null) hatch.Layer = layer;

                        outdxf.AddEntity(hatch);
                    }
                }
            }

            // check green fully contained into magenta
            {
                var q = faceGreen.Boolean(tol, faceMagenta).ToList();
                Assert.True(q.Count == 1 && q[0].Loops.Count == 1);
                var ecnt = q[0].Loops[0].Edges.Count;
                Assert.True(ecnt == loopGreen.Edges.Count);
                for (int i = 0; i < ecnt; ++i) Assert.True(loopGreen.Edges[i] == q[0].Loops[0].Edges[i]);
            }

            // check red fully contained into green
            {
                var q = faceGreen.Boolean(tol, faceRed).ToList();
                Assert.True(q.Count == 1 && q[0].Loops.Count == 1);
                var ecnt = q[0].Loops[0].Edges.Count;
                Assert.True(ecnt == loopRed.Edges.Count);
                for (int i = 0; i < ecnt; ++i) Assert.True(loopRed.Edges[i] == q[0].Loops[0].Edges[i]);
            }

            // blue not intersect green
            Assert.True(faceBlue.Boolean(tol, faceGreen).Count() == 0);

            // cyan-green *
            var faces = faceCyan.Boolean(tol, faceGreen).ToList();
            // dumpLoops(loops);            
            Assert.True(faces.Count == 2 && faces[0].Loops.Count == 1 && faces[1].Loops.Count == 1);
            ((Arc3D)faces[1].Loops[0].Edges[1]).Bulge(tol).AssertEqualsTol(1e-7, -0.21580411634390917);
            faces[0].Loops[0].Area.AssertEqualsTol(tol, 56.42492663);
            faces[0].Loops[0].Length.AssertEqualsTol(tol, 48.02672853);

            faces[1].Loops[0].Area.AssertEqualsTol(tol, 360.04194237);
            faces[1].Loops[0].Length.AssertEqualsTol(tol, 80.08303580);

            // green-cyan

            faces = faceGreen.Boolean(tol, faceCyan).ToList();

            Assert.True(faces.Count == 2 && faces[0].Loops.Count == 1 && faces[1].Loops.Count == 1);
            faces[0].Loops[0].Area.AssertEqualsTol(tol, 56.42492663);
            faces[0].Loops[0].Length.AssertEqualsTol(tol, 48.026728525416544);

            faces[1].Loops[0].Area.AssertEqualsTol(tol, 360.04194237);
            faces[1].Loops[0].Length.AssertEqualsTol(tol, 80.08303580);

            // green-yellow
            faces = faceGreen.Boolean(tol, faceYellow).ToList();

            Assert.True(faces.Count == 1 && faces[0].Loops.Count == 1);
            faces[0].Loops[0].Area.AssertEqualsTol(tol, 563.78939052);
            faces[0].Loops[0].Length.AssertEqualsTol(tol, 113.75563447);

            // yellow-green
            faces = faceYellow.Boolean(tol, faceGreen).ToList();

            Assert.True(faces.Count == 1 && faces[0].Loops.Count == 1);
            faces[0].Loops[0].Area.AssertEqualsTol(tol, 563.78939052);
            faces[0].Loops[0].Length.AssertEqualsTol(tol, 113.75563447);

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }

        }

    }

}
using Xunit;
using System.Linq;
using System;
using netDxf;
using netDxf.Entities;

namespace SearchAThing.Sci.Tests
{
    public partial class LoopTests
    {

        [Fact]
        public void LoopTest_0003()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0003.dxf"));

            DxfDocument? outdxf = null;
            //outdxf = new DxfDocument();

            var tol = 1e-8;

            var faceGreen = dxf.Entities.Polylines2D.First(w => w.Layer.Name == "green").ToLoop(tol).ToFace();
            var faceYellow = dxf.Entities.Polylines2D.First(w => w.Layer.Name == "yellow").ToLoop(tol).ToFace();

            var gyInts = faceGreen.Boolean(tol, faceYellow).ToList();

            Assert.True(gyInts.Count == 1 && gyInts[0].Loops.Count == 1);

            if (outdxf != null)
            {
                outdxf.AddEntity(faceGreen.Loops[0].DxfEntity(tol).Act(w => w.Layer = dxf.Layers.First(w => w.Name == "green")));
                outdxf.AddEntity(faceYellow.Loops[0].DxfEntity(tol).Act(w => w.Layer = dxf.Layers.First(w => w.Name == "yellow")));

                foreach (var gyInt in gyInts)
                {
                    outdxf.AddEntity(gyInt.Loops[0]
                        .ToHatch(tol, new HatchPattern(HatchPattern.Line.Name) { Angle = 45, Scale = 0.5 })
                        .SetColor(AciColor.Cyan));
                }
            }

            faceYellow.Loops[0].Area.AssertEqualsTol(tol, gyInts[0].Loops[0].Area);
            gyInts[0].Loops[0].Length.AssertEqualsTol(tol, 47.89069988);

            {
                var delta = new Vector3D(1, 2, 3);
                var movedTest = faceYellow.Move(delta);

                for (int i = 0; i < faceYellow.Loops[0].Edges.Count; ++i)
                {
                    (faceYellow.Loops[0].Edges[i].SGeomFrom + delta).AssertEqualsTol(tol, movedTest.Loops[0].Edges[i].SGeomFrom);
                    (faceYellow.Loops[0].Edges[i].SGeomTo + delta).AssertEqualsTol(tol, movedTest.Loops[0].Edges[i].SGeomTo);
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
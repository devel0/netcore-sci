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
        public void LoopTest_0006()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0006.dxf"));

            DxfDocument? outdxf = null;
            // outdxf = new DxfDocument();

            var tolFail = 0.1;
            var tolSuccess = 0.3;

            var tol = tolSuccess;

            var faceGreen = dxf.Entities.Polylines2D.First(w => w.Layer.Name == "green").ToLoop(tol).ToFace();
            var faceYellow = dxf.Entities.Polylines2D.First(w => w.Layer.Name == "yellow").ToLoop(tol).ToFace();

            // zoom at 0.0013,12.7559 to see geometry defect

            tol = tolFail;
            var gyInts = faceGreen.Boolean(tol, faceYellow).ToList();
            Assert.False(gyInts.Count == 1 && gyInts[0].Loops.Count == 1);

            tol = tolSuccess;
            gyInts = faceGreen.Boolean(tol, faceYellow).ToList();
            Assert.True(gyInts.Count == 1 && gyInts[0].Loops.Count == 1);

            if (outdxf != null)
            {
                outdxf.AddEntity(faceGreen.Loops[0].DxfEntity(tol).Act(w => w.Layer = new netDxf.Tables.Layer("int") { Color = AciColor.Red }));

                foreach (var gyInt in gyInts)
                {
                    outdxf.AddEntity(gyInt
                        .Loops[0]
                        .ToHatch(tol, new HatchPattern(HatchPattern.Line.Name) { Angle = 45, Scale = 0.5 })
                        .SetColor(AciColor.Cyan));
                }
            }

            gyInts[0].Loops[0].Area.AssertEqualsTol(tol, 33.05683422);
            gyInts[0].Loops[0].Length.AssertEqualsTol(tol, 51.48005902);

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }

        }

    }

}
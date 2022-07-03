using Xunit;
using System.Linq;
using System;
using netDxf;
using netDxf.Entities;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace SearchAThing.Sci.Tests
{
    public partial class LoopTests
    {

        [Fact]
        public void LoopTest_0011()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0011.dxf"));

            DxfDocument? outdxf = null;            
            outdxf = new DxfDocument();

            DxfDocument? outdxf2 = null;
            outdxf2 = new DxfDocument();

            var tol = 1e-8;

            var loopGreen = dxf.LwPolylines.First(w => w.Color.Index == AciColor.Green.Index).ToLoop(tol);
            var loopYellow = dxf.LwPolylines.First(w => w.Color.Index == AciColor.Yellow.Index).ToLoop(tol);
            var loopMagenta = dxf.LwPolylines.First(w => w.Color.Index == AciColor.Magenta.Index).ToLoop(tol);

            outdxf?.AddEntity(loopGreen.DxfEntity(tol).Set(x => x.SetColor(AciColor.Green)));
            outdxf?.AddEntity(loopYellow.DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow)));
            outdxf?.AddEntity(loopMagenta.DxfEntity(tol).Set(x => x.SetColor(AciColor.Magenta)));

            var yellowIntersectMagenta = loopYellow.Intersect(tol, loopMagenta).ToList();
            var greenIntersectMagenta = loopGreen.Intersect(tol, loopMagenta).ToList();

            Assert.True(yellowIntersectMagenta.Count == 1);
            yellowIntersectMagenta[0].Area.AssertEqualsTol(tol, 109.41172066);
            outdxf?.AddEntity(yellowIntersectMagenta[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow)));

            Assert.True(greenIntersectMagenta.Count == 1);
            greenIntersectMagenta[0].Area.AssertEqualsTol(tol, 88.60501699);
            outdxf?.AddEntity(greenIntersectMagenta[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Green)));

            var A = yellowIntersectMagenta[0];
            var B = greenIntersectMagenta[0];

            var yellowIntSubGreenInt = A.Intersect(tol, B, outdxf2).ToList();
            foreach (var res in yellowIntSubGreenInt)
            {
                outdxf?.AddEntity(res.DxfEntity(tol).Set(x => x.SetColor(AciColor.Red)));
            }
            ;
            //var gyInts = loopGreen.Intersect(tol, loopYellow).ToList();

            // Assert.True(gyInts.Count == 1);

            if (outdxf != null)
            {
                // outdxf.AddEntity(loopGreen.DxfEntity(tol).Set(w => w.Layer = dxf.Layers.First(w => w.Name == "green")));
                // outdxf.AddEntity(loopYellow.DxfEntity(tol).Set(w => w.Layer = dxf.Layers.First(w => w.Name == "yellow")));

                // foreach (var gyInt in gyInts)
                // {
                //     outdxf.AddEntity(gyInt
                //         .ToHatch(tol, new HatchPattern(HatchPattern.Line.Name) { Angle = 45, Scale = 0.5 })
                //         .SetColor(AciColor.Cyan));
                // }
            }

            // loopYellow.Area.AssertEqualsTol(tol, gyInts[0].Area);
            // gyInts[0].Length.AssertEqualsTol(tol, 47.89069988);

            // {
            //     var delta = new Vector3D(1, 2, 3);
            //     var movedTest = loopYellow.Move(delta);

            //     for (int i = 0; i < loopYellow.Edges.Count; ++i)
            //     {
            //         (loopYellow.Edges[i].SGeomFrom + delta).AssertEqualsTol(tol, movedTest.Edges[i].SGeomFrom);
            //         (loopYellow.Edges[i].SGeomTo + delta).AssertEqualsTol(tol, movedTest.Edges[i].SGeomTo);
            //     }
            // }

            if (outdxf != null)
            {
                outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf.Viewport.ShowGrid = false;
                outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
            }

            if (outdxf2 != null)
            {
                outdxf2.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
                outdxf2.Viewport.ShowGrid = false;
                outdxf2.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out2.dxf"));
            }
        }

    }

}
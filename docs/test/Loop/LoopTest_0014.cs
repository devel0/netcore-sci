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
        public void LoopTest_0014()
        {            
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0014.dxf"));

            DxfDocument? outdxf = null;
            // outdxf = new DxfDocument();

            DxfDocument? outdxf2 = null;
            // outdxf2 = new DxfDocument();

            var tol = 1e-8;

            var faceGreen = dxf.LwPolylines.First(w => w.Color.Index == AciColor.Green.Index).ToLoop(tol).ToFace();
            var faceYellow = dxf.LwPolylines.First(w => w.Color.Index == AciColor.Yellow.Index).ToLoop(tol).ToFace();

            outdxf?.AddEntity(faceGreen.Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Green)));
            outdxf?.AddEntity(faceYellow.Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow)));

            var sub = faceYellow.Boolean(tol, faceGreen, Face.BooleanMode.Difference, outdxf2).ToList();

            foreach (var res in sub)
            {
                outdxf?.AddEntity(res.Loops[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Red)));
                outdxf?.AddEntity(res
                    .Loops[0]
                    .ToHatch(tol, new HatchPattern(HatchPattern.Line.Name) { Angle = 45, Scale = 0.5 })
                    .SetColor(AciColor.Red));
            }

            Assert.True(sub.Count == 6);

            sub[0].Loops[0].Area.AssertEqualsTol(tol, 361.8427445113067);
            sub[0].Loops[0].Length.AssertEqualsTol(tol, 94.69225675761145);

            sub[1].Loops[0].Area.AssertEqualsTol(tol, 181.0339736153745);
            sub[1].Loops[0].Length.AssertEqualsTol(tol, 73.22649595561018);

            sub[2].Loops[0].Area.AssertEqualsTol(tol, 123.0062807844365);
            sub[2].Loops[0].Length.AssertEqualsTol(tol, 58.998642980826205);

            sub[3].Loops[0].Area.AssertEqualsTol(tol, 94.60509880947234);
            sub[3].Loops[0].Length.AssertEqualsTol(tol, 39.638105742989445);

            sub[4].Loops[0].Area.AssertEqualsTol(tol, 299.1853730731076);
            sub[4].Loops[0].Length.AssertEqualsTol(tol, 77.28269070361564);

            sub[5].Loops[0].Area.AssertEqualsTol(tol, 11.728056836031051);
            sub[5].Loops[0].Length.AssertEqualsTol(tol, 20.709388210898226);

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
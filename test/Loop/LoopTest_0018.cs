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
        public void LoopTest_0018()
        {            
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0018.dxf"));

            DxfDocument? outdxf = null;
            outdxf = new DxfDocument();

            DxfDocument? outdxf2 = null;
            outdxf2 = new DxfDocument();

            var tol = 1e-8;

            var faceYellow = dxf.LwPolylines.Where(w => w.Layer.Name == "yellow").ToFace(tol);
            var faceGreen = dxf.LwPolylines.Where(w => w.Layer.Name == "green").ToFace(tol);

            outdxf?.AddEntities(faceGreen.Loops.Select(loop => loop.DxfEntity(tol).Set(x => x.SetColor(AciColor.Green))));
            outdxf?.AddEntities(faceYellow.Loops.Select(loop => loop.DxfEntity(tol).Set(x => x.SetColor(AciColor.Yellow))));

            var layerRes = new netDxf.Tables.Layer("res") { Color = AciColor.Cyan };

            var res = faceYellow.Boolean(tol, faceGreen, Face.BooleanMode.Intersect, outdxf2).ToList();
            foreach (var face in res)
            {
                var hatch = face
                    .ToHatch(tol, new HatchPattern(HatchPattern.Line.Name) { Angle = 45, Scale = 0.5 })
                    ?.Set((eo, isBoundary) =>
                    {
                        eo.SetLayer(layerRes);
                        eo.Lineweight = isBoundary ? Lineweight.W13 : Lineweight.W0;
                    });
                if (hatch != null) outdxf?.AddEntity(hatch);
            }

            Assert.True(res.Count == 4);

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
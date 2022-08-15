using static System.Math;
using System;
using System.Linq;

using LinqStatistics;
using SearchAThing;
using System.Diagnostics;
using netDxf.Entities;
using netDxf;

namespace test
{
    class Program
    {

        static void Main(string[] args)
        {
            var tol = 1e-8;

            var inputPathfilename = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LoopTest_0002.dxf");
            var inputDxf = netDxf.DxfDocument.Load(inputPathfilename);

            var polys = inputDxf.Entities.Polylines2D.ToList();
            var lw1 = polys.First(w => w.Layer.Name == "green");
            var lw2 = polys.First(w => w.Layer.Name == "yellow");

            var face1 = lw1.ToLoop(tol).ToFace();
            var face2 = lw2.ToLoop(tol).ToFace();

            var ifaces = face1.Boolean(tol, face2).ToList();

            var outputPathfilename = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output.dxf");
            var outDxf = new netDxf.DxfDocument();

            foreach (var face in ifaces)
            {
                var ent = face.Loops[0].ToLwPolyline(tol);
                ent.Color = AciColor.Red;
                outDxf.AddEntity(ent);

                var hatch = face.Loops[0].ToHatch(tol,
                    HatchPattern.Line.Clone().Fn(o =>
                    {
                        var h = (HatchPattern)o;
                        h.Angle = 45;
                        h.Scale = 5;
                        return h;
                    }));
                hatch.Color = AciColor.Cyan;
                outDxf.AddEntity(hatch);

            }
            outDxf.AddEntities(new[] { (EntityObject)lw2.Clone(), (EntityObject)lw1.Clone() });

            outDxf.Viewport.ShowGrid = false;
            outDxf.Save(outputPathfilename);

            var psi = new ProcessStartInfo(outputPathfilename);
            psi.UseShellExecute = true;

            Process.Start(psi);
        }

    }

}
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

            var inputPathfilename = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "input.dxf");
            var inputDxf = netDxf.DxfDocument.Load(inputPathfilename);

            var polys = inputDxf.LwPolylines.ToList();
            var lw1 = polys[0];
            var lw2 = polys[1];

            var face1 = lw1.ToLoop(tol).ToFace();
            var face2 = lw2.ToLoop(tol).ToFace();

            var ifaces = face1.Boolean(tol, face2).ToList();

            var outputPathfilename = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output.dxf");
            var outDxf = new netDxf.DxfDocument();
            outDxf.AddEntities(new[] { (EntityObject)lw1.Clone(), (EntityObject)lw2.Clone() });
            foreach (var face in ifaces)
            {
                var ent = face.Loops[0].ToLwPolyline(tol);
                ent.Color = AciColor.Red;
                outDxf.AddEntity(ent);

                var hatch = face.Loops[0].ToHatch(tol,
                    HatchPattern.Line.Clone().Eval(o =>
                    {
                        HatchPattern h = (HatchPattern)o;
                        h.Angle = 45;
                        return h;
                    }));
                hatch.Color = AciColor.Yellow;
                outDxf.AddEntity(hatch);

            }
            outDxf.Viewport.ShowGrid = false;
            outDxf.Save(outputPathfilename);

            var psi = new ProcessStartInfo(outputPathfilename);
            psi.UseShellExecute = true;

            Process.Start(psi);
        }

    }

}
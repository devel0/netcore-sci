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

            var loop1 = lw1.ToLoop(tol);
            var loop2 = lw2.ToLoop(tol);

            var igeoms = loop1.Intersect(tol, loop2).ToList();

            var outputPathfilename = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output.dxf");
            var outDxf = new netDxf.DxfDocument();
            outDxf.AddEntities(new[] { (EntityObject)lw1.Clone(), (EntityObject)lw2.Clone() });
            foreach (var geom in igeoms)
            {
                // var ent = geom.DxfEntity;
                // ent.Color = AciColor.Red;
                // outDxf.AddEntity(ent);
            }
            outDxf.Viewport.ShowGrid = false;
            outDxf.Save(outputPathfilename);

            var psi = new ProcessStartInfo(outputPathfilename);
            psi.UseShellExecute = true;

            Process.Start(psi);
        }

    }

}
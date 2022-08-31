using static System.Math;
using SearchAThing;
using LinqStatistics;
using System;
using System.Linq;

namespace test
{
    class Program
    {

        static void Main(string[] args)
        {
            // var tol = 1e-8;
            var R = 100;

            var dxf = new netDxf.DxfDocument();
            var ang = 0d;
            var angStep = 10d.ToRad();
            var angElev = 20d.ToRad();

            var o = Vector3D.Zero;
            var p = new Vector3D(R, 0, 0);

            Circle3D circ = null;

            while (ang < 2 * PI)
            {
                var l = new Line3D(o, p.RotateAboutZAxis(ang));
                var l_ent = l.DxfEntity;
                l_ent.Color = netDxf.AciColor.Cyan;
                dxf.AddEntity(l_ent);

                var arcCS = new CoordinateSystem3D(o, l.V, Vector3D.ZAxis);
                var arc = new Arc3D(arcCS, R, 0, angElev);
                var arc_ent = arc.DxfEntity;
                arc_ent.Color = netDxf.AciColor.Yellow;
                dxf.AddEntity(arc_ent);

                var arc2CS = new CoordinateSystem3D(l.To - R * Vector3D.ZAxis,
                    Vector3D.ZAxis, Vector3D.Zero - l.To);
                var arc2 = new Arc3D(arc2CS, R, 0, PI / 2);
                var arc2_ent = arc2.DxfEntity;
                arc2_ent.Color = netDxf.AciColor.Green;
                dxf.AddEntity(arc2_ent);

                if (circ is null)
                {
                    circ = new Circle3D(
                        CoordinateSystem3D.WCS.Move(Vector3D.ZAxis * arc.To.Z),
                        arc.To.XYDistance(Vector3D.Zero));
                    var circ_ent = circ.DxfEntity;
                    circ_ent.Color = netDxf.AciColor.Yellow;
                    dxf.AddEntity(circ_ent);
                }

                ang += angStep;
            }

            dxf.Viewport.ShowGrid = false;
            dxf.Save("output.dxf", isBinary: true);
        }
    }
}

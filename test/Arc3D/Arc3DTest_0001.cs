using Xunit;
using System.Linq;
using static System.Math;
using System;
using SearchAThing;

namespace SearchAThing.Sci.Tests
{
    public partial class Arc3DTests
    {

        [Fact]
        public void Arc3DTest_0001()
        {
            var tol = 1e-8;

            var p1 = new Vector3D("X = 26.07344765 Y = 126.09450063 Z = -29.43767056");
            var p2 = new Vector3D("X = 75.89230418 Y = 224.11406806 Z = 35.97437873");
            var p3 = new Vector3D("X = 113.96165191 Y = 223.78464949 Z = -9.6695621");

            var arc = new Arc3D(tol, p1, p2, p3);

            var plo = new Vector3D("X = -74.96786784 Y = 178.50832685 Z = -43.45380285");
            var plx = new Vector3D("X = 16.59432595 Y = 276.19478284 Z = -28.05497445");
            var ply = new Vector3D("X = -15.76912557 Y = 115.16567263 Z = 6.37808524");

            var plcs = new CoordinateSystem3D(plo, plx - plo, ply - plo);

            var ips = arc.Intersect(tol, plcs);
            Console.WriteLine($"ips cnt = {ips.Count()}");
            ;

        }
    }
}
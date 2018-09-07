using Xunit;
using System.Linq;
using SearchAThing.Util;

namespace SearchAThing.Sci.Tests
{
    public class Arc3DTests
    {

        double rad_tol;

        public Arc3DTests()
        {
            rad_tol = (1e-1).ToRad();
        }

        [Fact]
        public void Arc3DTest()
        {
            var o = new Vector3D(4.4249, 1.0332, -3.7054);
            var cs = new CoordinateSystem3D(o,
                new Vector3D(-.1201, 4.4926, 1.4138),
                new Vector3D(-1.6282, 3.2952, 2.1837));

            var r = 4.2753;
            var ang1 = 26.10878d.ToRad();
            var ange2 = 63.97731d.ToRad();
            var arc = new Arc3D(cs, r, ang1, ange2);

            var seg_i = new Line3D(2.2826, 10.2516, -3.7469, 1.3767, -3.7709, 1.5019);
            var ip_circ = arc.Intersect(1e-4, seg_i);
            var ip_arc = arc.IntersectArc(1e-2, rad_tol, seg_i);

            // aegment intersecting arc
            Assert.True(ip_circ.Count() == 1);
            Assert.True(ip_circ.First().EqualsTol(1e-4, ip_arc.First()));
            Assert.True(ip_circ.First().EqualsTol(1e-2, 1.83, 3.24, -1.12));

            // segment not intersecting arc
            var seg_i2 = seg_i.Scale(seg_i.From, .3);
            Assert.True(arc.Intersect(1e-4, seg_i2, segment_mode: true).Count() == 0);
            Assert.True(arc.IntersectArc(1e-4, rad_tol, seg_i2, segment_mode: true).Count() == 0);

            // segment not intersecting, but line intersecting arc
            Assert.True(arc.Intersect(1e-4, seg_i2).Count() == 1);
            Assert.True(arc.Intersect(1e-4, seg_i2).First().EqualsTol(1e-4, ip_arc.First()));

            // segment intresecting circle, but not arc
            var seg_e = new Line3D(2.4523, 9.6971, -1.8004, 4.6142, -.0631, -2.0519);
            Assert.True(arc.Intersect(1e-4, seg_e).Count() == 1);
            Assert.True(arc.Intersect(1e-4, seg_e).First().EqualsTol(1e-2, 3.53, 4.82, -1.93));
            Assert.True(arc.IntersectArc(1e-4, rad_tol, seg_e).Count() == 0);
        }

    }

}
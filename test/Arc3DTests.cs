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

        /// <summary>
        /// Arc3DTest_001.dxf
        /// </summary>
        [Fact]
        public void Arc3DTest_001()
        {
            var o = new Vector3D(4.4249, 1.0332, -3.7054);
            var cs_ = new CoordinateSystem3D(o,
                new Vector3D(-.1201, 4.4926, 1.4138),
                new Vector3D(-1.6282, 3.2952, 2.1837));

            // build cs for arc with same cad representation
            var csCAD = new CoordinateSystem3D(o, cs_.BaseZ, CoordinateSystem3DAutoEnum.AAA);

            var r = 4.2753;
            var ang1 = 63.97731d.ToRad();
            var ange2 = 26.10878d.ToRad();
            var arc = new Arc3D(csCAD, r, ang1, ange2);

            var seg_i = new Line3D(2.2826, 10.2516, -3.7469, 1.3767, -3.7709, 1.5019);
            var ip_circ = arc.Intersect(1e-4, seg_i);
            var ip_arc = arc.IntersectArc(1e-2, rad_tol, seg_i);

            // segment intersecting arc
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

        /// <summary>
        /// Arc3DTest_002.dxf
        /// </summary>
        [Fact]
        public void Arc3DTest_002()
        {
            var p1 = new Vector3D(20.175, 178.425, -56.314);
            var p2 = new Vector3D(1.799, 231.586, -18.134);
            var p3 = new Vector3D(262.377, 302.118, 132.195);

            var c = Arc3D.CircleBy3Points(p1, p2, p3);
            var cs = c.CS;

            // verify points contained in arc plane
            Assert.True(cs.Contains(1e-3, p1));
            Assert.True(cs.Contains(1e-3, p2));
            Assert.True(cs.Contains(1e-3, p3));

            // cscad retrieved from cad using ucs align entity
            var cscad = new CoordinateSystem3D(new Vector3D(165.221, 214.095, 24.351),
                new Vector3D(-0.259, -0.621, 0.74), CoordinateSystem3DAutoEnum.AAA);

            // assert cs and cscad are coplanar with same origin
            Assert.True(cscad.Origin.EqualsTol(1e-3, cs.Origin));
            Assert.True((cscad.BaseX + cs.Origin).ToUCS(cs).Z.EqualsTol(1e-3, 0));
            Assert.True((cscad.BaseY + cs.Origin).ToUCS(cs).Z.EqualsTol(1e-3, 0));

            Assert.True(c.Radius.EqualsTol(1e-3, 169.758));

            // create a circle through p1,p2,p3 and states if the same as arc by 3 points
            var c2 = new Circle3D(p1, p2, p3);
            Assert.True(c.Radius.EqualsAutoTol(c2.Radius));
            Assert.True(c.CS.Origin.EqualsAutoTol(c2.CS.Origin));
            Assert.True(c.CS.IsParallelTo(1e-3, c2.CS));
        }

        /// <summary>
        /// Arc3DTest_003.dxf
        /// </summary>
        [Fact]
        public void Arc3DTest_003()
        {
            var p1 = new Vector3D(20.175, 178.425, -56.314);
            var p2 = new Vector3D(1.799, 231.586, -18.134);
            var p3 = new Vector3D(262.377, 302.118, 132.195);

            var c = Arc3D.CircleBy3Points(p1, p2, p3);
            var cs = c.CS;

            var arc = new Arc3D(1e-3, p1, p2, p3, -cs.BaseZ);
            var arcCs = arc.CS;

            // two cs share same origin
            Assert.True(arc.CS.Origin.EqualsTol(1e-3, cs.Origin));
            // two cs with discordant colinear Z
            Assert.True(arc.CS.BaseZ.Colinear(1e-3, cs.BaseZ) && !arc.CS.BaseZ.Concordant(1e-3, cs.BaseZ));
            // two cs parallel
            Assert.True(arc.CS.IsParallelTo(1e-3, cs));
        }

    }

}
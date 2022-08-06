using Xunit;
using System.Linq;
using System;
using static System.Math;
using System.Collections.Generic;

namespace SearchAThing.Sci.Tests
{
    public partial class Arc3DTests_torefact
    {

        public Arc3DTests_torefact()
        {
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
            var c = new Circle3D(arc);

            var seg_i = new Line3D(2.2826, 10.2516, -3.7469, 1.3767, -3.7709, 1.5019);
            var ip_circ = c.Intersect(1e-4, seg_i, segment_mode: false);
            var ip_arc = arc.Intersect(1e-2, seg_i);

            // segment intersecting arc
            Assert.True(ip_circ.Count() == 1);
            Assert.True(ip_circ.First().EqualsTol(1e-4, ip_arc.First()));
            Assert.True(ip_circ.First().EqualsTol(1e-2, 1.83, 3.24, -1.12));

            // segment not intersecting arc
            var seg_i2 = seg_i.Scale(seg_i.From, .3);
            Assert.True(c.Intersect(1e-4, seg_i2, segment_mode: true).Count() == 0);
            Assert.True(arc.Intersect(1e-4, seg_i2, segment_mode: true).Count() == 0);

            // segment not intersecting, but line intersecting arc
            Assert.True(c.Intersect(1e-4, seg_i2, segment_mode: false).Count() == 1);
            Assert.True(c.Intersect(1e-4, seg_i2, segment_mode: false).First().EqualsTol(1e-4, ip_arc.First()));

            // segment intresecting circle, but not arc
            var seg_e = new Line3D(2.4523, 9.6971, -1.8004, 4.6142, -.0631, -2.0519);
            Assert.True(c.Intersect(1e-4, seg_e, segment_mode: false).Count() == 1);
            Assert.True(c.Intersect(1e-4, seg_e, segment_mode: false).First().EqualsTol(1e-2, 3.53, 4.82, -1.93));
            Assert.True(arc.Intersect(1e-4, seg_e).Count() == 0);

            // Geometry
            Assert.True(arc.GeomFrom.EqualsTol(1e-3, arc.From));
            Assert.True(arc.GeomTo.EqualsTol(1e-3, arc.To));
            var vertexes = arc.Vertexes.ToList();
            Assert.True(vertexes.Count == 2 &&
                vertexes.Any(a => a.EqualsTol(1e-3, arc.From)) &&
                vertexes.Any(a => a.EqualsTol(1e-3, arc.To)));

            // PtAngle and mid
            var midpoint = arc.MidPoint;
            Assert.True(arc.PtAtAngle(arc.AngleStart + arc.Angle / 2).EqualsTol(1e-3, midpoint));

            // segment
            Assert.True(arc.Segment.EqualsTol(1e-3, new Line3D(arc.From, arc.To)));

            // arc equals
            Assert.True(arc.EqualsTol(1e-3, new Arc3D(1e-3, arc.From, arc.MidPoint, arc.To)));
            Assert.True(arc.EqualsTol(1e-3, new Arc3D(arc.CS, arc.Radius, arc.AngleStart, arc.AngleEnd)));
            Assert.False(arc.EqualsTol(1e-3, new Arc3D(arc.CS, arc.Radius / 2, arc.AngleStart, arc.AngleEnd)));
            Assert.False(arc.EqualsTol(1e-3, new Arc3D(arc.CS.Move(new Vector3D(1, 0, 0)),
                arc.Radius, arc.AngleStart, arc.AngleEnd)));
            Assert.False(arc.EqualsTol(1e-3, new Arc3D(arc.CS, arc.Radius, arc.AngleStart + 1, arc.AngleEnd + 1)));

            // arc bulge            
            arc.Bulge(1e-3).AssertEqualsTol(1e-3, Tan(arc.Angle / 4));

            // arc contains            
            Assert.False(arc.Contains(1e-3, new Vector3D(3.084, 3.965, -1.843), onlyPerimeter: false)); // out arc shape - in plane
            Assert.False(arc.Contains(1e-3, new Vector3D(2.821, 4.417, -2.795), onlyPerimeter: false)); // out of plane

            Assert.True(arc.Contains(1e-3, new Vector3D(5.446, 3.708, -3.677), onlyPerimeter: false)); // arc shape - in plane
            Assert.False(arc.Contains(1e-3, new Vector3D(5.142, 3.774, -4.713), onlyPerimeter: false)); // out of plane            

            // read arc from dxf and use comparer
            {
                var doc = netDxf.DxfDocument.Load("_torefact/doc/Arc3DTest_001.dxf");

                var arc_from_dxf = doc.Entities.Arcs.First().ToArc3D();
                var cmp = new Arc3DEqualityComparer(1e-3);
                var q = new[] {
                    arc_from_dxf,
                    arc,
                    new Arc3D(1e-3, arc.From, arc.MidPoint, arc.To),
                    new Arc3D(arc.CS, arc.Radius, arc.AngleStart, arc.AngleEnd) };
                Assert.True(q.Distinct(cmp).Count() == 1);

                Assert.True(arc_from_dxf.EqualsTol(1e-3, ((netDxf.Entities.Arc)arc.DxfEntity).ToArc3D()));
            }
        }

        /// <summary>
        /// Arc3DTest_002.dxf
        /// </summary>
        [Fact]
        public void Arc3DTest_002()
        {
            var tol = 1e-3;

            var p1 = new Vector3D(20.175, 178.425, -56.314);
            var p2 = new Vector3D(1.799, 231.586, -18.134);
            var p3 = new Vector3D(262.377, 302.118, 132.195);

            var c = new Arc3D(tol, p1, p2, p3);
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
        /// Arc3DTest_005.dxf
        /// </summary>
        [Fact]
        public void Arc3DTest_005()
        {
            var p1 = new Vector3D(20.17459383, 178.42487311, -56.31435851);
            var p2 = new Vector3D(1.7990927, 231.58612295, -18.13420814);
            var p3 = new Vector3D(262.37695212, 302.11773752, 132.19450446);

            var c = new Arc3D(1e-3, p1, p2, p3);
            Assert.True(new Line3D(c.Center, c.CS.BaseX, Line3DConstructMode.PointAndVector)
                .SemiLineContainsPoint(1e-3, p1));
            var radTol = (1e-1).RadTol(c.Radius);
            var degTol = radTol.ToDeg();
            {
                var cinverse = new Arc3D(1e-3, p3, p2, p1);
                Assert.True(new Line3D(c.Center, cinverse.CS.BaseX, Line3DConstructMode.PointAndVector)
                    .SemiLineContainsPoint(1e-3, p3));
                Assert.True(c.AngleStart.EqualsTol(radTol, cinverse.AngleStart));
                Assert.True(c.AngleEnd.EqualsTol(radTol, cinverse.AngleEnd));
            }

            Assert.True(c.AngleStart.ToDeg().EqualsTol(degTol, 0));
            Assert.True(c.AngleEnd.ToDeg().EqualsTol(degTol, 154.14));

            Assert.True(c.Contains(1e-3, p1, onlyPerimeter: true));
            Assert.True(c.Contains(1e-3, p2, onlyPerimeter: true));
            Assert.True(c.Contains(1e-3, p3, onlyPerimeter: true));

            var moveVector = new Vector3D(-1998.843, -6050.954, -1980.059);
            var cmoved = c.Move(moveVector);

            Assert.True(cmoved is Arc3D);

            var p1moved = p1 + moveVector;
            var p2moved = p2 + moveVector;
            var p3moved = p3 + moveVector;

            Assert.True(cmoved.Contains(1e-3, p1moved, onlyPerimeter: true));
            Assert.True(cmoved.Contains(1e-3, p2moved, onlyPerimeter: true));
            Assert.True(cmoved.Contains(1e-3, p3moved, onlyPerimeter: true));

            Assert.True(c.Angle.ToDeg().EqualsTol(degTol, 154.14));

            var c2 = new Arc3D(c.CS, c.Radius, c.AngleEnd, c.AngleStart);

            Assert.True(c2.Angle.ToDeg().EqualsTol(degTol, 360d - 154.14));

            Assert.True(c.Length.EqualsTol(1e-3, 456.67959116));
        }

        /// <summary>
        /// Arc3DTest_006.dxf
        /// </summary>
        [Fact]
        public void Arc3DTest_006()
        {
            var tol = 1e-7;

            var p1 = new Vector3D(20.17459383, 178.42487311, -56.31435851);
            var p2 = new Vector3D(1.7990927, 231.58612295, -18.13420814);
            var p3 = new Vector3D(262.37695212, 302.11773752, 132.19450446);
            var arc = new Arc3D(tol, p1, p2, p3);

            // area and centre of mass
            var A = 0d;
            var centroid = arc.CentreOfMass(out A);
            Assert.True(A.EqualsTol(1e-7, 32476.83673649));
            Assert.True(centroid.EqualsTol(1e-7, new Vector3D("X=106.62109106 Y=278.15563166 Z=57.60718457")));

            var dp1 = new Vector3D("X = 4.11641325 Y = 266.06066703 Z = 11.60392802");
            var dp2 = new Vector3D("X = 58.22323201 Y = 331.06393108 Z = 85.07377904");
            var dp3 = new Vector3D("X = 158.93019908 Y = 345.12414417 Z = 132.0972665");
            {
                var q = arc.Divide(4, include_endpoints: true).ToList();
                Assert.True(q.Count == 3 + 2);
                Assert.True(q.Any(w => w.EqualsTol(tol, p1)));
                Assert.True(q.Any(w => w.EqualsTol(tol, dp1)));
                Assert.True(q.Any(w => w.EqualsTol(tol, dp2)));
                Assert.True(q.Any(w => w.EqualsTol(tol, dp3)));
                Assert.True(q.Any(w => w.EqualsTol(tol, p3)));
            }

            // bbox
            var bbox = arc.BBox(tol);
            Assert.True(bbox.Contains(tol, new Vector3D("X = 1.24800294 Y = 178.42487311 Z = -56.31435851")));
            Assert.True(bbox.Contains(tol, new Vector3D("X = 262.37695212 Y = 178.42487311 Z = -56.31435851")));
            Assert.True(bbox.Contains(tol, new Vector3D("X = 262.37695212 Y = 347.16710407 Z = -56.31435851")));
            Assert.True(bbox.Contains(tol, new Vector3D("X = 1.24800294 Y = 347.16710407 Z = -56.31435851")));
            Assert.True(bbox.Contains(tol, new Vector3D("X = 1.24800294 Y = 178.42487311 Z = 138.54160976")));
            Assert.True(bbox.Contains(tol, new Vector3D("X = 262.37695212 Y = 178.42487311 Z = 138.54160976")));
            Assert.True(bbox.Contains(tol, new Vector3D("X = 262.37695212 Y = 347.16710407 Z = 138.54160976")));
            Assert.True(bbox.Contains(tol, new Vector3D("X = 1.24800294 Y = 347.16710407 Z = 138.54160976")));
        }

        /// <summary>
        /// Arc3DTest_007.dxf
        /// </summary>
        [Fact]
        public void Arc3DTest_007()
        {
            var tol = 1e-7;

            var p1 = new Vector3D(20.17459383, 178.42487311, -56.31435851);
            var p2 = new Vector3D(1.7990927, 231.58612295, -18.13420814);
            var p3 = new Vector3D(262.37695212, 302.11773752, 132.19450446);
            var arc = new Arc3D(tol, p1, p2, p3);

            var cso = new Vector3D("X = -153.32147396 Y = 128.44203407 Z = -156.2065643");
            var csv1 = new Line3D(cso, new Vector3D("X = 71.66072643 Y = 278.03911571 Z = -156.2065643"));
            var csv2 = new Line3D(cso, new Vector3D("X = -153.32147396 Y = 128.44203407 Z = 2.05865164"));

            Assert.True(arc.Intersect(1e-6, arc.CS.Move(arc.CS.BaseZ * 1000)).Count() == 0);

            var csplane = new CoordinateSystem3D(cso, csv1.V, csv2.V);
            var ipts = arc.Intersect(tol, csplane).ToList();
            Assert.True(ipts.Count == 2);
            var ipLine = new Line3D(ipts[0], ipts[1]);
            Assert.True(ipts.Any(r => r.EqualsTol(tol, new Vector3D("X = 1.7990927 Y = 231.58612296 Z = -18.13420814"))));
            Assert.True(ipts.Any(r => r.EqualsTol(tol, new Vector3D("X = 169.80266871 Y = 343.29649219 Z = 134.36668758"))));
        }

        /// <summary>
        /// Arc3DTest_008.dxf
        /// </summary>
        [Fact]
        public void Arc3DTest_008()
        {
            var tol = 1e-7;

            var p1 = new Vector3D(20.17459383, 178.42487311, -56.31435851);
            var p2 = new Vector3D(1.7990927, 231.58612295, -18.13420814);
            var p3 = new Vector3D(262.37695212, 302.11773752, 132.19450446);
            var arc = new Arc3D(tol, p1, p2, p3);

            var dp1 = new Vector3D("X = 4.11641325 Y = 266.06066703 Z = 11.60392802");
            var dp2 = new Vector3D("X = 58.22323201 Y = 331.06393108 Z = 85.07377904");
            var dp3 = new Vector3D("X = 158.93019908 Y = 345.12414417 Z = 132.0972665");
            var dp4 = new Vector3D("X = 62.63561614 Y = 268.87580225 Z = 34.43511732");
            var dp4_ = new Vector3D("X = 16.03656748 Y = 293.76000567 Z = 39.01589812");

            Action<Arc3D, Vector3D, Vector3D> test = (subarc, ap1, ap2) =>
            {
                Assert.True(arc.CS.Origin.EqualsTol(tol, subarc.CS.Origin));
                Assert.True(arc.CS.IsParallelTo(tol, subarc.CS));
                Assert.True(subarc.From.EqualsTol(tol, ap1));
                Assert.True(subarc.To.EqualsTol(tol, ap2));
            };

            Assert.True(arc.Split(tol, null).Count() == 0);
            Assert.True(arc.Split(tol, new Vector3D[] { }).Count() == 0);
            Assert.True(arc.Split(tol, new[] { dp1, dp2, dp3, dp4, arc.From, arc.To }).Count() == 5);

            {
                var dps = new[] { dp1, dp2, dp3, dp4 };
                var subarcs = arc.Split(tol, dps).ToList();
                Assert.True(subarcs.Count == 5);

                test(subarcs[0], arc.From, dp1);
                test(subarcs[1], dp1, dp4_);
                test(subarcs[2], dp4_, dp2);
                test(subarcs[3], dp2, dp3);
                test(subarcs[4], dp3, arc.To);
            }

            {
                var dps = new[] { dp1, dp2, dp3, dp4 };
                var subarcs = arc.Split(tol, dps, validate_pts: true).ToList();
                Assert.True(subarcs.Count == 4);

                test(subarcs[0], arc.From, dp1);
                test(subarcs[1], dp1, dp2);
                test(subarcs[2], dp2, dp3);
                test(subarcs[3], dp3, arc.To);
            }

        }

    }

}
using Xunit;
using static System.Math;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Globalization;
using ClipperLib;
using SearchAThing.Util;
using static SearchAThing.Sci.MUCollection;

namespace SearchAThing.Sci.Tests
{
    public class OldTests
    {
/*
        // tests from https://github.com/SearchAThing-old1/SearchAThing.UnitTest

        IModel model = new SampleModel();

        [Fact(DisplayName = "DoubleEqualityComparer")]
        public void DoubleEqualityComparerTest()
        {
            var dcmp = new DoubleEqualityComparer(1e-4);
            var a = -0.00050000000000000044;
            var b = -0.000499999999999997;
            Assert.True(dcmp.Equals(a, b) && dcmp.GetHashCode(a) == dcmp.GetHashCode(b));
        }

        [Fact(DisplayName = "Reduce")]
        public void ReduceTest()
        {
            var tol = .1;
            var input = new[] { -1.1, -0.5, -0.55, -1.17, -1.21, 2, 2.1, 2.2, 2.3, 2.46, 2.5, 5 };
            var q = input.Thin(tol);
            var expected_res = new[] { -1.21, -1.1, -0.5, 2, 2.1, 2.2, 2.46, 5 };
            Assert.True(q.Count == expected_res.Length);
            for (int i = 0; i < expected_res.Length; ++i)
            {
                Assert.True(q[i].EqualsTol(tol, expected_res[i]));
            }
        }

        [Fact(DisplayName = "Vector3D")]
        public void Vector3DTest()
        {
            var tolLen = model.MUDomain.Length.DefaultTolerance;
            var tolRad = model.MUDomain.PlaneAngle.DefaultTolerance;

            // length
            Assert.True(new Vector3D(1, 5.9, 4).Length.EqualsTol(tolLen, 7.198));

            // normalized
            Assert.True(new Vector3D(1, 5.9, 4).Normalized().EqualsTol(Constants.NormalizedLengthTolerance, new Vector3D(0.13893, 0.81968, 0.55572)));

            // distance
            Assert.True(new Vector3D(1, 5.9, 4).Distance(new Vector3D(3, 4.3, 1.03)).EqualsTol(tolLen, 3.9218));

            // dot product
            Assert.True(new Vector3D(5, 1, 3).DotProduct(new Vector3D(5, 4, 6)).EqualsTol(tolLen, 47));

            // cross product
            Assert.True(new Vector3D(2, 4, 12).CrossProduct(new Vector3D(3, 6, 1)).EqualsTol(tolLen, new Vector3D(-68, 34, 0)));

            // angle rad
            Assert.True(new Vector3D(3.48412, 2.06577, 0).AngleRad(tolLen, new Vector3D(1.4325, 2.70248, 0)).EqualsTol(tolRad, 0.548));
            Assert.True(new Vector3D(.231334209442139, .143270492553711)
                .AngleRad(Constants.NormalizedLengthTolerance, new Vector3D(-.224979639053345, -.153055667877197))
                .EqualsTol(tolRad, (177.54306).ToRad()));

            // angle rad
            Assert.True(new Vector3D(3.48412, 2.06577, 0).AngleRad(tolLen, new Vector3D(-3.48412, -2.066, 0)).EqualsTol(tolRad, PI));

            // angle contained
            Assert.True(340d.ToRad().AngleInRange(tolRad, 330d.ToRad(), 3d.ToRad()));
            Assert.True(0d.ToRad().AngleInRange(tolRad, 330d.ToRad(), 3d.ToRad()));

            // vector projection
            Assert.True(new Vector3D(101.546, 25.186, 1.3).Project(new Vector3D(48.362, 46.564, 5))
                .EqualsTol(tolLen, new Vector3D(64.9889, 62.5728, 6.719)));

            // vector vers
            Assert.True(new Vector3D(101.546, 25.186, 1.3).Concordant(tolLen, new Vector3D(50.773, 12.593, .65)));
            Assert.False(new Vector3D(101.546, 25.186, 1.3).Concordant(tolLen, new Vector3D(-50.773, -12.593, .65)));

            // angle toward
            Assert.True(new Vector3D(120.317, 42.914, 0).AngleToward(tolLen, new Vector3D(28.549, 63.771, 0), Vector3D.ZAxis)
                .EqualsTol(MUCollection.PlaneAngle.rad.Tolerance(model), 0.80726));

            Assert.False(new Vector3D(120.317, 42.914, 0).AngleToward(tolLen, new Vector3D(28.549, 63.771, 0), -Vector3D.ZAxis)
                .EqualsTol(MUCollection.PlaneAngle.rad.Tolerance(model), 0.80726));

            Assert.True(new Vector3D(120.317, 42.914, 0).AngleToward(tolLen, new Vector3D(28.549, 63.771, 0), -Vector3D.ZAxis)
                .EqualsTol(MUCollection.PlaneAngle.rad.Tolerance(model), 2 * PI - 0.80726));

            Assert.True(Abs(
                new Vector3D(-6.95, -5.1725, 0).AngleToward(1e-6, new Vector3D(6.95, 5.1725, 0), new Vector3D(0, 0, 71.89775))
                - PI)
                < model.MUDomain.PlaneAngle.DefaultTolerance);

            // z-axis rotation
            Assert.True(new Vector3D(109.452, 38.712, 0).RotateAboutZAxis((50.0).ToRad())
                .EqualsTol(tolLen, new Vector3D(40.6992, 108.7286, 0)));

            // arbitrary axis rotation
            Assert.True(new Vector3D(747.5675, 259.8335, 0).RotateAboutAxis(new Vector3D(123.151, 353.8977, 25.6), (50.0).ToRad())
                .EqualsTol(tolLen, new Vector3D(524.3462, 370.9603, -462.4069)));

            // rotate relative
            Assert.True(
                new Vector3D(69.1831, 157.1155, 300).RotateAs(tolLen,
                    new Vector3D(443.6913, 107.8843, 0), new Vector3D(342.7154, 239.6307, 0))
                .EqualsTol(tolLen, new Vector3D(7.3989, 171.5134, 300)));

            // vector parallel (1-d)
            Assert.True(new Vector3D(1, 0, 0).IsParallelTo(tolLen, new Vector3D(-1, 0, 0)));
            Assert.True(new Vector3D(0, 1, 0).IsParallelTo(tolLen, new Vector3D(0, -2, 0)));
            Assert.True(new Vector3D(0, 0, 1).IsParallelTo(tolLen, new Vector3D(0, 0, -3)));

            // vector parallel (2-d)
            Assert.True(new Vector3D(1, 1, 0).IsParallelTo(tolLen, new Vector3D(-2, -2, 0)));
            Assert.False(new Vector3D(1, 1, 0).IsParallelTo(tolLen, new Vector3D(-2, 2, 0)));

            Assert.True(new Vector3D(1, 0, 1).IsParallelTo(tolLen, new Vector3D(-2, 0, -2)));
            Assert.False(new Vector3D(1, 0, 1).IsParallelTo(tolLen, new Vector3D(-2, 0, 2)));

            Assert.True(new Vector3D(0, 1, 1).IsParallelTo(tolLen, new Vector3D(0, -2, -2)));
            Assert.False(new Vector3D(0, 1, 1).IsParallelTo(tolLen, new Vector3D(0, -2, 2)));

            // vector parallel (3-d)
            Assert.True(new Vector3D(1, 1, 1).IsParallelTo(tolLen, new Vector3D(-2, -2, -2)));
            Assert.True(new Vector3D(253.6625, 162.6347, 150).IsParallelTo(tolLen, new Vector3D(380.4937, 243.952, 225)));

            // vector parallel ( tolerance checks )
            {
                // ensure mm tolerance 1e-1
                var tmpModel = new SampleModel();

                var tmpTolLen = 1e-1;

                // Z component of first vector will be considered zero cause < 1e-1 model tolerance     
                Assert.True(new Vector3D(10, 0, 0.05).IsParallelTo(tmpTolLen, new Vector3D(-2, 0, 0)));

                // Z component of first vector will be considered not-zero cause > 1e-1 model tolerance
                Assert.False(new Vector3D(10, 0, 0.11).IsParallelTo(tmpTolLen, new Vector3D(-2, 0, 0)));

                // X, Z components of first vector force internal usage of normalized tolerance 1e-4
                // cause the length of the shortest vector here is < 1.5 and may represents a normalized vector
                // or a result of such type of operation
                Assert.True(new Vector3D(0.09, 0, 0.09).IsParallelTo(tmpTolLen, new Vector3D(-20, 0, -20)));
            }
        }

        [Fact(DisplayName = "Helpers")]
        void HelpersTest()
        {
            // MRound
            Assert.True((1.32).MRound(.2).EqualsTol(1e-6, 1.4));
            Assert.True((56781.52).MRound(125).EqualsTol(1e-6, 56750));

            // Magnitude
            Assert.True((190.0).Magnitude() == 2);
            Assert.True((.0034).Magnitude() == -3);

            Assert.True((0.1).Magnitude() == -1);
            Assert.True((0.0).Magnitude() == 0);
            Assert.True((1.0).Magnitude() == 0);
            Assert.True((10.0).Magnitude() == 1);

            // RoundTol
            Assert.True((1923.56789).MRound(1e-1).EqualsTol(1e-6, 1923.6));
            Assert.True((1923.52789).MRound(1e-1).EqualsTol(1e-6, 1923.5));
        }

        [Fact(DisplayName = "Vector3DCmp")]
        void Vector3DCmp()
        {
            var tol = 1e-3;
            var cmp = new Vector3DEqualityComparer(tol);

            {
                var rnd = new Random();
                var lst = new List<Vector3D>();
                for (int i = 0; i < 150; ++i)
                {
                    lst.Add(new Vector3D(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble()));
                }
                var last = lst.Last();
                var someIn = new Vector3D(last.X, last.Y, last.Z); // different reference

                var K = 100000;
                var cnt = 0;

                var hs = new HashSet<Vector3D>(); // with-out Vector3DCompare it search for same ref
                lst.ForEach(w => hs.Add(w));
                for (int j = 0; j < K; ++j)
                {
                    if (hs.Contains(someIn)) ++cnt;
                }
                Assert.True(cnt == 0); // and it don't found

                var sw1 = new Stopwatch();
                sw1.Start();
                hs = new HashSet<Vector3D>(cmp); // using the comparer it search for value
                lst.ForEach(w => hs.Add(w));
                cnt = 0;
                for (int j = 0; j < K; ++j)
                {
                    if (hs.Contains(someIn)) ++cnt;
                }
                Assert.True(cnt == K);
                sw1.Stop();

                // using list it should run slower
                var sw2 = new Stopwatch();
                sw2.Start();
                cnt = 0;
                for (int j = 0; j < K; ++j)
                {
                    if (lst.Any(r => r.EqualsTol(tol, someIn))) // word case
                        ++cnt;
                }
                Assert.True(cnt == K);
                sw2.Stop();

                // sw1.Elapsed	{00:00:00.0195037}	System.TimeSpan
                // sw2.Elapsed	{00:00:01.0507771}	System.TimeSpan

                Assert.True(sw1.Elapsed < sw2.Elapsed);
            }

            {
                var lst = Vector3D.From3DCoords(
                    .004, .45, 30
                    );

                // tol is 1e-3
                Assert.Contains(new Vector3D(.004, .45, 30), lst, cmp);
                Assert.Contains(new Vector3D(.0049, .45, 30), lst, cmp);
                Assert.DoesNotContain(new Vector3D(.0051, .45, 30), lst, cmp);
            }

            {
                var v1 = new Vector3D(10, 0, 0);
                var v2 = new Vector3D(9.9999999999999716, 0, 0);
                var vcmp = new Vector3DEqualityComparer(1e-4);
                Assert.True(vcmp.GetHashCode(v1) == vcmp.GetHashCode(v2));
                Assert.True(vcmp.Equals(v1, v2));
            }
        }

        [Fact(DisplayName = "Line3D")]
        void Line3DTest()
        {
            var tolLen = model.MUDomain.Length.DefaultTolerance;

            // line contains point
            {
                var tolLenExcess = tolLen + 1e-10;

                // line (0,0,0)-(1,0,0)
                var l = new Line3D(Vector3D.Zero, new Vector3D(1, 0, 0), Line3DConstructMode.PointAndVector);
                Assert.True(l.LineContainsPoint(tolLen, 2, 0, 0));
                Assert.False(l.LineContainsPoint(tolLen, 2, 1, 0));
                Assert.False(l.LineContainsPoint(tolLen, 2, 0, 1));

                Assert.True(l.SegmentContainsPoint(tolLen, 1, 0, 0));
                Assert.True(l.SegmentContainsPoint(tolLen, 0, 0, 0));
                Assert.False(l.SegmentContainsPoint(tolLen, -tolLenExcess, 0, 0));

                Assert.True(l.SegmentContainsPoint(tolLen, 0, tolLen, 0));
                Assert.False(l.SegmentContainsPoint(tolLen, 0, tolLenExcess, 0));

                var l2 = new Line3D(Vector3D.Zero, new Vector3D(5, 0, 0), Line3DConstructMode.PointAndVector);
                // point on line
                Assert.True(l2.SegmentContainsPoint(.5, new Vector3D(-.5, 0, 0)));
                Assert.False(l2.SegmentContainsPoint(.5, new Vector3D(-.5 - 1e-10, 0, 0)));
            }

            // line 3d intersection
            {
                {
                    var l1 = new Line3D(new Vector3D(0, 0, 0), new Vector3D(1, 0, 0));
                    var l2 = new Line3D(new Vector3D(2, 0, 0), new Vector3D(2, 0, 2));

                    Assert.True(l1.Intersect(tolLen, l2).EqualsTol(tolLen, 2, 0, 0));
                }

                {
                    var l1 = new Line3D(new Vector3D(0, 0, 0), new Vector3D(0, 1, 0));
                    var l2 = new Line3D(new Vector3D(0, 2, 0), new Vector3D(2, 2, 0));

                    Assert.True(l1.Intersect(tolLen, l2).EqualsTol(tolLen, 0, 2, 0));
                }

                {
                    var l1 = new Line3D(new Vector3D(0, 0, 0), new Vector3D(0, 0, 1));
                    var l2 = new Line3D(new Vector3D(0, 0, 2), new Vector3D(2, 0, 2));

                    Assert.True(l1.Intersect(tolLen, l2).EqualsTol(tolLen, 0, 0, 2));
                }

                {
                    var l1 = new Line3D(new Vector3D(0, 0, 0), new Vector3D(1.6206, 2, -1.4882));
                    var l2 = new Line3D(new Vector3D(1.2, .7, 2), new Vector3D(.6338, .3917, .969));

                    Assert.True(l1.Intersect(tolLen, l2).EqualsTol(tolLen, 0.0675, 0.0833, -0.062));
                }
            }

            // project point on a line
            {
                var p = new Vector3D(1, 1, 0);
                var perpLine = Line3D.XAxisLine.Perpendicular(tolLen, p);
                Assert.True(perpLine.From.EqualsTol(tolLen, p) && perpLine.To.EqualsTol(tolLen, 1, 0, 0));
            }

            // check two lines are colinear
            {
                Assert.True(new Line3D(new Vector3D(0, 0, 0), new Vector3D(1, 1, 1))
                    .Colinear(tolLen, new Line3D(new Vector3D(2, 2, 2), new Vector3D(3, 3, 3))));

                Assert.False(new Line3D(new Vector3D(0, 0, 0), new Vector3D(1, 1, 1))
                    .Colinear(tolLen, new Line3D(new Vector3D(0, 0, 0), new Vector3D(1, 1, 1.11))));

                {
                    var v1 = Vector3D.From2DCoords(22.646652351539, 13.9522716251755, 23.5544912108728, 15.1454145967669);
                    var v2 = Vector3D.From2DCoords(32.1714356467077, 26.4617928826826, 23.5548859449172, 15.1451142433282);
                    var s1 = new Line3D(v1[0], v1[1]);
                    var s2 = new Line3D(v2[0], v2[1]);
                    Assert.True(s1.Colinear(1e-3, s2) == s2.Colinear(1e-3, s1));
                }

                {
                    var s1 = new Line3D(-90, 29.4932764928483, -90, 40);
                    var s2 = new Line3D(-90, -2.84E-14, -90, 80);
                    var segs = new[] { s1, s2 };
                    Assert.True(segs.MergeColinearSegments(1e-3).Count() == 1);
                }
            }

            // merge segments
            {
                var sega = new Line3D(new Vector3D(0, 0, 0), new Vector3D(2, 0, 0));
                var segb = new Line3D(new Vector3D(1, 0, 0), new Vector3D(3, 0, 0));
                var segc = new Line3D(new Vector3D(3, 0, 0), new Vector3D(1, 0, 0));

                {
                    var merge = new List<Line3D>() { sega, segb }.MergeColinearSegments(tolLen).ToList();
                    Assert.True(merge.Count == 1 && merge.First().EqualsTol(tolLen, new Line3D(new Vector3D(0, 0, 0), new Vector3D(3, 0, 0))));
                }
                {
                    var merge = new List<Line3D>() { sega, segc }.MergeColinearSegments(tolLen).ToList();
                    Assert.True(merge.Count == 1 && merge.First().EqualsTol(tolLen, new Line3D(new Vector3D(0, 0, 0), new Vector3D(3, 0, 0))));
                }
            }
        }

        [Fact(DisplayName = "Matrix3D")]
        void Matrix3DTest()
        {
            var m = new Matrix3D(new double[] {
                1, .5, 6,
                .1, 2, .05,
                .7, 11, .55
            });

            // det
            Assert.True(m.Determinant().EqualsTol(1e-6, -1.260));

            // inv
            Assert.True(m.Inverse().EqualsTol(1e-3, new Matrix3D(new double[] {
                -0.437, -52.163, 9.504,
                0.016, 2.897, -0.437,
                0.238, 8.452, -1.548
            })));

            // solve
            Assert.True(m.Solve(1.1, 2.2, 3.3).EqualsTol(1e-3, new Vector3D(-83.875, 4.95, 13.75)));
        }

        [Fact(DisplayName = "Plane3D")]
        void Plane3DTest()
        {
            var tol = model.MUDomain.Length.DefaultTolerance;

            var plane = new Plane3D(new CoordinateSystem3D(Vector3D.Zero, Vector3D.XAxis, Vector3D.YAxis));

            var q = new Line3D(new Vector3D(1, -3, -5), new Vector3D(10, 4, 6)).Intersect(tol, plane);

            Assert.True(q.EqualsTol(tol, 5.0909, 0.1818, 0));

            Assert.True(plane.CS.IsParallelTo(tol, new Plane3D(new CoordinateSystem3D(
                new Vector3D(0, 0, 1), new Vector3D(-1, 0, 0), new Vector3D(0, -1, 0))).CS));
        }

        [Fact(DisplayName = "Circle3D")]
        void Circle3DTest()
        {
            var tol = model.MUDomain.Length.DefaultTolerance;

            {
                var circle = new Circle3D(
                    new Vector3D(52.563, 177.463, 0),
                    new Vector3D(180.94, 258.505, 0),
                    new Vector3D(297.124, 112.916, 50));

                Assert.True(circle.Radius.EqualsTol(tol, 129.6516));
                Assert.True(circle.Area.EqualsTol(tol, 52808.7467));
                Assert.True(circle.Center.EqualsTol(tol, 170.9181, 132.1797, 27.4052));
                Assert.True(circle.CS.BaseX.EqualsTol(Constants.NormalizedLengthTolerance, -0.9128, 0.3492, -0.2113));
                Assert.True(circle.CS.BaseY.EqualsTol(Constants.NormalizedLengthTolerance, 0.3837, 0.9107, -0.1526));
            }

            {
                tol = 2e-4;

                var center = new Vector3D(22.51209044, 14.12016062, 6.80146259);
                var N = new Vector3D(3.42020143, 2.43210347, -9.07673371);
                var c = new Circle3D(tol, new CoordinateSystem3D(center, N), 6.39114677);

                var ltangent = new Line3D(
                    new Vector3D(12.87708949, 20.20572433, 4.80152252),
                    new Vector3D(30.90622113, 20.43217568, 11.65575191));
                var lcoplanar = new Line3D(
                    new Vector3D(13.64224068, 16.47774921, 4.09093100),
                    new Vector3D(30.67186845, 16.07799217, 10.40074559));
                var l1point = new Line3D(
                    new Vector3D(3.39057372, 13.97172849, -2.64691872),
                    new Vector3D(29.98782817, 12.34242299, 11.34548562));
                var lout = new Line3D(
                    new Vector3D(12.61618025, 9.11647552, 5.03700782),
                    new Vector3D(29.98782817, 8.95992883, 10.43914904));

                var ips = c.Intersect(tol, ltangent).ToList();
                Assert.True(ips.Count == 1 && ips[0].EqualsTol(tol, 21.89165531, 20.31895000, 8.22863722));

                ips = c.Intersect(tol, lcoplanar).ToList();
                Assert.True(ips.Count == 2 &&
                    ips.Any(w => w.EqualsTol(tol, 16.75040000, 16.40478763, 5.24256563)) &&
                    ips.Any(w => w.EqualsTol(tol, 27.98738426, 16.14100832, 9.40609100)));

                ips = c.Intersect(tol, l1point).ToList();
                Assert.True(ips.Count == 1 && ips[0].EqualsTol(tol, 16.68920095, 13.15707574, 4.34928345));

                ips = c.Intersect(tol, lout).ToList();
                Assert.True(ips.Count == 0);
            }

            {
                tol = 1e-3;
                var p = new Vector3D(3.29679008, 9.48216033, 0);
                var t = new Line3D(new Vector3D(18.14266304, 4.72460258, 0), new Vector3D(1.29059103, 1.80128397, 0));
                var r = 9.8191;

                var circles = Circle3D.CircleRTanP(tol, p, t, r);

                Assert.True(circles.Count() == 2);
                Assert.True(circles.All(w => w.Radius.EqualsTol(tol, 9.81913052)));
                Assert.Contains(circles, (w) => w.Center.EqualsTol(tol, 12.18378030, 13.65597387));
                Assert.Contains(circles, (w) => w.Center.EqualsTol(tol, -6.47673267, 10.41894611));
            }
        }

        [Fact(DisplayName = "Polygon")]
        void PolygonTest()
        {
            {
                var tolLen = model.MUDomain.Length.DefaultTolerance;

                var B = 700; var b = 50;
                var H = 900; var h = 50;

                var pts = new List<Vector3D>()
                {
                    new Vector3D(0, 0, 0), new Vector3D(B, 0, 0),
                    new Vector3D(B, h, 0),new Vector3D(b, h, 0),
                    new Vector3D(b, H+h, 0), new Vector3D(B, H+h, 0),
                    new Vector3D(B, H +2*h, 0), new Vector3D(0, H+2*h, 0)
                };

                // area
                var area = pts.Area(tolLen);
                Assert.True(area.EqualsTol(tolLen, 2 * B * h + H * b));

                // centroid
                Assert.True(pts.Centroid(tolLen, area).EqualsTol(tolLen, (2 * h * B * B / 2 + b * b * H / 2) / area, H / 2 + h, 0));

                // segment contains point
                // inner
                Assert.True(pts.ContainsPoint(tolLen, new Vector3D(28.173, 275.518, 0)));
                Assert.False(pts.ContainsPoint(tolLen, new Vector3D(78.044, 312.565, 0)));

                // on perimeter
                Assert.True(Vector3D.From3DCoords(1.36144026245989, 99.1876096460911, 0, 85.8743599084552, 6.25033248506968, 0, 64.2455555332105, 2.86368127114311, 0)
                    .ContainsPoint(1e-1, new Vector3D(64.245555533210535, 2.8636812711431094, 0)));

                // on right segment
                Assert.True(pts.ContainsPoint(tolLen, new Vector3D(b, (H + 2 * h) / 2, 0)));

                // on left segment
                Assert.True(pts.ContainsPoint(tolLen, new Vector3D(0, (H + 2 * h) / 2, 0)));
            }

            {
                var tol = 1e-3;

                {
                    var pts = Vector3D.From2DCoords(
                        0, 0,
                        10, 0,
                        10, 10,
                        0, 10
                        );
                    Assert.True(pts.Centroid(tol).EqualsTol(tol, 5, 5, 0));

                    pts = pts.Select(w => w - new Vector3D(10, 10)).ToList();
                    Assert.True(pts.Centroid(tol).EqualsTol(tol, -5, -5, 0));
                }

                {
                    var pts = Vector3D.From2DCoords(
                        0, 2,
                        33, 2,
                        33, 35,
                        0, 35
                        );
                    var A = pts.Area(tol);
                    Assert.True(A.EqualsTol(1e-1, 1089));
                    Assert.True(pts.Centroid(tol).EqualsTol(tol, 16.5, 18.5, 0));
                }

                {
                    var pts = Vector3D.From2DCoords(
                        0, 2,
                        0, 35,
                        33, 35,
                        33, 2
                        );
                    var A = pts.Area(tol);
                    Assert.True(A.EqualsTol(1e-1, 1089));
                    Assert.True(pts.SortPoly(tol, Vector3D.ZAxis).ToList().Centroid(tol).EqualsTol(tol, 16.5, 18.5, 0));
                }

            }

            {
                var tol = 1e-2;

                var pts = Vector3D.From2DCoords(
                    68.12, 78.93,
                    71.86, 75.28,
                    71.68, 74.92,
                    71.18, 74.56,
                    67.03, 78.61);

                Assert.True(pts.Area(tol).EqualsTol(5.51624999, 1e-6));
            }

            {
                var tol = 1e-4;

                var pts = Vector3D.From3DCoords(82864.6156987284, 50220.4741441392, 0, 82865.1044856623, 50219.6005979311, 0, 82865.7272074748, 50219.9865042438, 0, 82865.6869543679, 50220.5655944049, 0, 82865.14866095, 50220.8044258826, 0);
                var pt = new Vector3D(82868.5968081355, 50220.4742179776, 0);

                Assert.False(pts.ContainsPoint(tol, pt));
            }

            {
                var tol = 1e-4;

                var pts = Vector3D.From3DCoords(82864.4498220986, 50219.2717508153, 0, 82865.1044856623, 50219.6005979311, 0, 82864.6156987284, 50220.4741441392, 0, 82864.0553965054, 50220.1926961096, 0, 82863.9773540763, 50219.6089969349, 0);
                var pt = new Vector3D(82868.5968081355, 50220.4742179776, 0);

                Assert.False(pts.ContainsPoint(tol, pt));
            }

            {
                var tol = 1e-4;

                var pts = Vector3D.From3DCoords(82865.1486609555, 50220.8044259296, 0, 82865.1367431909, 50221.4029804696, 0, 82864.5665724475, 50221.62015258, 0, 82864.126911018, 50221.3476917443, 0, 82864.6156997685, 50220.4741466723, 0);
                var pt = new Vector3D(82868.5968081355, 50220.474217977557, 0);

                Assert.False(pts.ContainsPoint(tol, pt));
            }

            {
                var tol = 1e-4;

                var pts = Vector3D.From3DCoords(82806.2853751313, 50287.8496284662, 0, 82806.9567465798, 50287.1038154201, 0, 82804.856242037, 50285.5083768389, 0, 82804.856242037, 50285.5083768389, 0, 82804.3249042247, 50286.3605524571, 0, 82804.3249042247, 50286.3605524571, 0, 82806.2853751312, 50287.8496284662, 0);
                var pt = new Vector3D(82821.621856159938, 50304.487467415951);

                Assert.False(pts.ContainsPoint(tol, pt, zapDuplicates: true));
            }

        }

        [Fact(DisplayName = "ClosedPolys2D")]
        void ClosedPolys2D()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"), "test_closed_polys1.dxf"));

            var segs = dxf.Lines.Select(w => w.ToLine3D()).ToList();
            segs = segs.AutoIntersect(1e-3).ToList();

            var polys = segs.ClosedPolys2D(1e-3).ToList();
            Assert.True(polys.Count == 9);
            Assert.True(polys.All(r => r.ToList().Area(1e-3).EqualsTol(1e-3, 1111.1111)));
        }

        [Fact(DisplayName = "SortPolygon")]
        void SortPolygonTest()
        {
            var tol = model.MUDomain.Length.DefaultTolerance;

            {
                var p1 = new Vector3D(-19.331, 168.749, 0);
                var p2 = new Vector3D(95.57, 90.108, 0);
                var p3 = new Vector3D(286.757, 182.876, 0);
                var p4 = new Vector3D(149.253, 277.528, 0);
                var p5 = new Vector3D(29.173, 265.755, 0);

                var pts = new List<Vector3D>() { p4, p2, p5, p3, p1 };

                var pts2 = pts.SortPoly(tol).ToList();

                Assert.True(
                    pts2[0].EqualsTol(tol, p3) &&
                    pts2[1].EqualsTol(tol, p2) &&
                    pts2[2].EqualsTol(tol, p1) &&
                    pts2[3].EqualsTol(tol, p5) &&
                    pts2[4].EqualsTol(tol, p4));
            }

            {
                var pts = Vector3D.From2DCoords(
                    100, 500,
                    300, 700,
                    300, 500,
                    100, 700);

                var pts2 = pts.SortPoly(tol).ToList();

                Assert.True(pts2.EqualsTol(tol, Vector3D.From2DCoords(
                    300, 500,
                    300, 700,
                    100, 700,
                    100, 500)));

                pts2 = pts.SortPoly(tol, -Vector3D.ZAxis).ToList();
                Assert.True(pts2.EqualsTol(tol, Vector3D.From2DCoords(
                    100, 700,
                    300, 700,
                    300, 500,
                    100, 500)));
            }

            {
                var pts = Vector3D.From2DCoords(
                    -1, -3.96,
                    12.9, -3.96,
                    -1, 6.3850000000000007,
                    12.9, 6.3850000000000007
                    );

                var pts2 = pts.SortPoly(1e-6).ToList();

                Assert.True(pts2.EqualsTol(tol, Vector3D.From2DCoords(
                    12.9, -3.96,
                    12.9, 6.385,
                    -1, 6.385,
                    -1, -3.96)));
            }
        }

        [Fact(DisplayName = "BBox3D")]
        void BBox3DTest()
        {
            var tolLen = model.MUDomain.Length.DefaultTolerance;

            var pts = Vector3D.From3DCoords(
                1, 5, 8,
                -2, -.5, 9,
                10, .5, .9);

            var bbox = pts.BBox();

            Assert.True(bbox.Min.EqualsTol(tolLen, -2, -.5, .9));
            Assert.True(bbox.Max.EqualsTol(tolLen, 10, 5, 9));

            var bbox2 = new BBox3D().Union(new Vector3D(1, 2, 3));
            Assert.True(bbox.EqualsTol(tolLen, bbox.Union(bbox2)));

            Assert.True(bbox.Contains(tolLen, bbox2));
        }

        [Fact(DisplayName = "MeasureUnit")]
        void MeasureUnitTest()
        {
            {

                var measure = Measure.TryParse("10mm [Length]");
                Assert.True(measure != null &&
                    measure.MU == MUCollection.Length.mm &&
                    measure.MU.PhysicalQuantity == PQCollection.Length &&
                    Abs(measure.Value - 10) < 1e-3);
            }

            var mud = new MUDomain();

            // Length
            {
                var tol = mud.Length.DefaultTolerance;

                var mm = MUCollection.Length.mm;
                var m = MUCollection.Length.m;
                var km = MUCollection.Length.km;
                var inch = MUCollection.Length.inch;
                var ft = MUCollection.Length.ft;
                var yard = MUCollection.Length.yard;
                var links = MUCollection.Length.links;

                var a = (212356.435 * mm).ConvertTo(mud).Value;

                Assert.True(a.EqualsTol(tol, (212356.435).Convert(mm, mud)));
                Assert.True(a.EqualsTol(tol, (212.356435).Convert(m, mud)));
                Assert.True(a.EqualsTol(tol, (0.212356435).Convert(km, mud)));
                Assert.True(a.EqualsTol(tol, (8360.489567).Convert(inch, mud)));
                Assert.True(a.EqualsTol(tol, (696.7074639).Convert(ft, mud)));
                Assert.True(a.EqualsTol(tol, (232.2358213).Convert(yard, mud)));
                Assert.True(a.EqualsTol(tol, (1055.6173695617595).Convert(links, mud)));
            }

            // Temperature
            {
                var tol = mud.Temperature.DefaultTolerance;

                var C = MUCollection.Temperature.C;
                var K = MUCollection.Temperature.K;
                var F = MUCollection.Temperature.F;

                var C_ = 20.35;
                var K_ = 293.5;
                var F_ = 68.63;

                {
                    var T = (C_ * C);

                    Assert.True(T.ConvertTo(K).Value.EqualsTol(1e-1, K_));
                    Assert.True(T.ConvertTo(F).Value.EqualsTol(1e-2, F_));
                }

                {
                    var T = (K_ * K);

                    Assert.True(T.ConvertTo(C).Value.EqualsTol(1e-2, C_));
                    Assert.True(T.ConvertTo(F).Value.EqualsTol(1e-2, F_));
                }

                {
                    var T = (F_ * F);

                    Assert.True(T.ConvertTo(C).Value.EqualsTol(1e-2, C_));
                    Assert.True(T.ConvertTo(K).Value.EqualsTol(1e-1, K_));
                }

            }

            // Linear acceleration
            {
                var a = 4.4 * MUCollection.Acceleration.mm_s2;
                var b = a.ConvertTo(MUCollection.Acceleration.m_s2).Value;
                Assert.True(b.EqualsTol(1e-4, 0.0044));
            }

            // Angular speed
            {
                var a = (2.5) * MUCollection.AngularSpeed.rad_s;
                var b = a.ConvertTo(MUCollection.AngularSpeed.deg_s);
                Assert.True(b.Value.EqualsTol(1e-3, 143.239));

                Assert.True(AngularAcceleration.Auto(MUCollection.PlaneAngle.deg, MUCollection.Time.sec)
                    .Equals(MUCollection.AngularSpeed.deg_s));
                Assert.True(AngularAcceleration.Auto(MUCollection.PlaneAngle.rad, MUCollection.Time.sec)
                    .Equals(MUCollection.AngularSpeed.rad_s));
            }

            // Angular acceleration
            {
                var a = (2.5) * MUCollection.AngularAcceleration.rad_s2;
                var b = a.ConvertTo(MUCollection.AngularAcceleration.deg_s2);
                Assert.True(b.Value.EqualsTol(1e-3, 143.239));

                Assert.True(AngularAcceleration.Auto(MUCollection.PlaneAngle.deg, MUCollection.Time.sec)
                    .Equals(MUCollection.AngularAcceleration.deg_s2));
                Assert.True(AngularAcceleration.Auto(MUCollection.PlaneAngle.rad, MUCollection.Time.sec)
                    .Equals(MUCollection.AngularAcceleration.rad_s2));
            }

            // Bending moment
            {
                var Nmm = MUCollection.BendingMoment.Auto(MUCollection.Force.N, MUCollection.Length.mm);
                Assert.True(Nmm.Equals(MUCollection.BendingMoment.Nmm));

                var a = 1.78 * Nmm;
                var b = a.ConvertTo(MUCollection.BendingMoment.kNm);
                Assert.True(b.Value.EqualsTol(1e-8, 1.78e-6));
            }

        }

        [Fact(DisplayName = "Line3DAutoIntersect")]
        void Line3DAutoIntersect()
        {
            var dxf = netDxf.DxfDocument.Load(
                System.IO.Path.Combine(
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"),
                    "test_autointersect.dxf"));

            var input_segs = dxf.Lines
                .Where(r => r.Layer.Name == "lay_input")
                .Select(w => w.ToLine3D()).ToList();

            var processed_segs = input_segs.AutoIntersect(1e-4);

            var expected_segs = dxf.Lines
                .Where(r => r.Layer.Name == "lay_ok")
                .Select(w => w.ToLine3D()).ToList();

            var tolLen = 1e-4;

            var q1 = processed_segs.OrderBy(w => w.MidPoint.ToString(tolLen)).ToList();
            var q2 = expected_segs.OrderBy(w => w.MidPoint.ToString(tolLen)).ToList();

            Assert.True(q1.Count == q2.Count);
            for (int i = 0; i < q1.Count; ++i)
            {
                Assert.True(q1[i].EqualsTol(tolLen, q2[i]));
            }

        }

        [Fact(DisplayName = "NumberTest")]
        void NumberTest()
        {
            Assert.True((3.1415926535897931).NormalizeAngle2PI(1e-12) == (3.1415926535897931));
        } 

        [Fact(DisplayName = "PolyBool")]
        public void PolyBool()
        {
            var poly1 = Vector3D.From2DCoords(2456959.24317107, 4521194.78205498, 2456959.24292108, 4521194.80140498, 2456959.24219605, 4521194.82065498, 2456959.24099609, 4521194.83975498, 2456959.23929605, 4521194.85872998, 2456959.23714605, 4521194.87757998, 2456959.23452106, 4521194.89627997, 2456959.23144606, 4521194.91482997, 2456959.22794607, 4521194.93320497, 2456959.2239711, 4521194.95142997, 2456959.21957108, 4521194.96950497, 2456959.21472105, 4521194.98737997, 2456959.2094461, 4521195.00507997, 2456959.20377109, 4521195.02260496, 2456959.19767109, 4521195.03992995, 2456959.19114611, 4521195.05705496, 2456959.18424609, 4521195.07400497, 2456959.16922107, 4521195.10720496, 2456959.1526461, 4521195.13955497, 2456959.13459608, 4521195.17095496, 2456959.11509606, 4521195.20137994, 2456959.09417108, 4521195.23080494, 2456959.07192108, 4521195.25912996, 2456959.04834607, 4521195.28635492, 2456959.02349606, 4521195.31237992, 2456958.99747106, 4521195.33722993, 2456958.97024611, 4521195.36080494, 2456958.94192109, 4521195.38305493, 2456958.91249609, 4521195.40397992, 2456958.88207111, 4521195.42347993, 2456958.85067111, 4521195.44152995, 2456958.81832111, 4521195.45810493, 2456958.78512111, 4521195.47312995, 2456958.7681711, 4521195.48002996, 2456958.75104609, 4521195.48655494, 2456958.73372111, 4521195.49265494, 2456958.71619612, 4521195.49832996, 2456958.69849612, 4521195.50360491, 2456958.68062112, 4521195.50845494, 2456958.66254611, 4521195.51285495, 2456958.64432111, 4521195.51682993, 2456958.62594612, 4521195.52032991, 2456958.60739612, 4521195.52340492, 2456958.58869612, 4521195.5260299, 2456958.56984612, 4521195.5281799, 2456958.55087112, 4521195.52987995, 2456958.53177112, 4521195.53107991, 2456958.51252112, 4521195.53180494, 2456958.49317113, 4521195.53205492, 2456958.47382113, 4521195.53180494, 2456958.45457112, 4521195.53107991, 2456958.43547113, 4521195.52987995, 2456958.41649612, 4521195.5281799, 2456958.39764612, 4521195.5260299, 2456958.37894613, 4521195.52340492, 2456958.36042112, 4521195.52032991, 2456958.34202112, 4521195.51682993, 2456958.32379612, 4521195.51285495, 2456958.30574612, 4521195.50845494, 2456958.28784612, 4521195.50360491, 2456958.27014612, 4521195.49832996, 2456958.25262113, 4521195.49265494, 2456958.23529613, 4521195.48655494, 2456958.21817112, 4521195.48002996, 2456958.20124611, 4521195.47312995, 2456958.16802111, 4521195.45810493, 2456958.13567111, 4521195.44152995, 2456958.10427111, 4521195.42347993, 2456958.07384613, 4521195.40397992, 2456958.04444611, 4521195.38305493, 2456958.01609612, 4521195.36080494, 2456957.9888961, 4521195.33722993, 2456957.96284613, 4521195.31237992, 2456957.93802109, 4521195.28635492, 2456957.91444609, 4521195.25912996, 2456957.89217112, 4521195.23080494, 2456957.8712711, 4521195.20137994, 2456957.85174611, 4521195.17095496, 2456957.8336961, 4521195.13955497, 2456957.81712112, 4521195.10720496, 2456957.80212113, 4521195.07400497, 2456957.79519609, 4521195.05705496, 2456957.78867111, 4521195.03992995, 2456957.78257111, 4521195.02260496, 2456957.77689609, 4521195.00507997, 2456957.77162114, 4521194.98737997, 2456957.76679614, 4521194.96950497, 2456957.76237109, 4521194.95142997, 2456957.75842109, 4521194.93320497, 2456957.75489613, 4521194.91482997, 2456957.75182113, 4521194.89627997, 2456957.74919614, 4521194.87757998, 2456957.74704614, 4521194.85872998, 2456957.7453461, 4521194.83975498, 2456957.74414614, 4521194.82065498, 2456957.74342111, 4521194.80140498, 2456957.74317113, 4521194.78205498, 2456957.74342111, 4521194.76270498, 2456957.74414614, 4521194.74345498, 2456957.7453461, 4521194.72435498, 2456957.74704614, 4521194.70537998, 2456957.74919614, 4521194.68652998, 2456957.75182113, 4521194.66782998, 2456957.75489613, 4521194.64930498, 2456957.75842109, 4521194.63090498, 2456957.76237109, 4521194.61267998, 2456957.76679614, 4521194.59462998, 2456957.77162114, 4521194.57672997, 2456957.77689609, 4521194.55902997, 2456957.78257111, 4521194.54150498, 2456957.78867111, 4521194.52417998, 2456957.79519609, 4521194.50705497, 2456957.80212113, 4521194.49012997, 2456957.81712112, 4521194.45690497, 2456957.8336961, 4521194.42455496, 2456957.85174611, 4521194.39315497, 2456957.8712711, 4521194.36272999, 2456957.89217112, 4521194.33332997, 2456957.91444609, 4521194.30497997, 2456957.93802109, 4521194.27777996, 2456957.96284613, 4521194.25172998, 2456957.9888961, 4521194.22690495, 2456958.01609612, 4521194.20332994, 2456958.04444611, 4521194.18105497, 2456958.07384613, 4521194.16015496, 2456958.10427111, 4521194.14062997, 2456958.13567111, 4521194.12257995, 2456958.16802111, 4521194.10600497, 2456958.20124611, 4521194.09100499, 2456958.21817112, 4521194.08407994, 2456958.23529613, 4521194.07755496, 2456958.25262113, 4521194.07145496, 2456958.27014612, 4521194.06577994, 2456958.28784612, 4521194.06050499, 2456958.30574612, 4521194.05568, 2456958.32379612, 4521194.05125495, 2456958.34202112, 4521194.04730495, 2456958.36042112, 4521194.04377999, 2456958.37894613, 4521194.04070499, 2456958.39764612, 4521194.03808, 2456958.41649612, 4521194.03593, 2456958.43547113, 4521194.03422995, 2456958.45457112, 4521194.03302999, 2456958.47382113, 4521194.03230496, 2456958.49317113, 4521194.03205498, 2456958.51252112, 4521194.03230496, 2456958.53177112, 4521194.03302999, 2456958.55087112, 4521194.03422995, 2456958.56984612, 4521194.03593, 2456958.58869612, 4521194.03808, 2456958.60739612, 4521194.04070499, 2456958.62594612, 4521194.04377999, 2456958.64432111, 4521194.04730495, 2456958.66254611, 4521194.05125495, 2456958.68062112, 4521194.05568, 2456958.69849612, 4521194.06050499, 2456958.71619612, 4521194.06577994, 2456958.73372111, 4521194.07145496, 2456958.75104609, 4521194.07755496, 2456958.7681711, 4521194.08407994, 2456958.78512111, 4521194.09100499, 2456958.81832111, 4521194.10600497, 2456958.85067111, 4521194.12257995, 2456958.88207111, 4521194.14062997, 2456958.91249609, 4521194.16015496, 2456958.94192109, 4521194.18105497, 2456958.97024611, 4521194.20332994, 2456958.99747106, 4521194.22690495, 2456959.02349606, 4521194.25172998, 2456959.04834607, 4521194.27777996, 2456959.07192108, 4521194.30497997, 2456959.09417108, 4521194.33332997, 2456959.11509606, 4521194.36272999, 2456959.13459608, 4521194.39315497, 2456959.1526461, 4521194.42455496, 2456959.16922107, 4521194.45690497, 2456959.18424609, 4521194.49012997, 2456959.19114611, 4521194.50705497, 2456959.19767109, 4521194.52417998, 2456959.20377109, 4521194.54150498, 2456959.2094461, 4521194.55902997, 2456959.21472105, 4521194.57672997, 2456959.21957108, 4521194.59462998, 2456959.2239711, 4521194.61267998, 2456959.22794607, 4521194.63090498, 2456959.23144606, 4521194.64930498, 2456959.23452106, 4521194.66782998, 2456959.23714605, 4521194.68652998, 2456959.23929605, 4521194.70537998, 2456959.24099609, 4521194.72435498, 2456959.24219605, 4521194.74345498, 2456959.24292108, 4521194.76270498, 2456959.24317107, 4521194.78205498);
            var poly2 = Vector3D.From2DCoords(2456958.93739697, 4521193.96433002, 2456958.71923788, 4521193.8783273, 2456958.45056103, 4521193.85033209, 2456956.88572247, 4521200.66687345, 2456957.63172698, 4521199.65191578);

            var q = poly1.Boolean(1e-4, poly2, ClipType.ctIntersection);

            Assert.True(q.Count() != 0);
        }
*/
    }

}

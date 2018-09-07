using Xunit;
using System.Linq;
using static System.Math;
using System;
using SearchAThing.Util;

namespace SearchAThing.Sci.Tests
{
    public class Vector3DTests
    {

        double rad_tol;

        public Vector3DTests()
        {
            rad_tol = (1e-1).ToRad();
        }

        [Fact]
        public void AngleRadTest()
        {
            var v1 = new Vector3D(10, 0, 0);
            var v2 = new Vector3D(2, 5, 0);
            var angv1v2 = v1.AngleRad(1e-4, v2);
            var angv2v1 = v2.AngleRad(1e-4, v1);
            Assert.True(angv1v2.EqualsTol(rad_tol, angv2v1));
            Assert.True(angv1v2.EqualsTol(rad_tol, 68.2d.ToRad()));
        }

        [Fact]
        public void AngleTowardTest()
        {
            var v1 = new Vector3D(10, 0, 0);
            var v2 = new Vector3D(2, 5, 0);

            var angv1v2_zplus = v1.AngleToward(1e-4, v2, Vector3D.ZAxis);
            var angv1v2_zminus = v1.AngleToward(1e-4, v2, -Vector3D.ZAxis);

            var angv2v1_zplus = v2.AngleToward(1e-4, v1, Vector3D.ZAxis);
            var angv2v1_zminus = v2.AngleToward(1e-4, v1, -Vector3D.ZAxis);

            Assert.True(angv1v2_zplus.EqualsTol(rad_tol, angv2v1_zminus));
            Assert.True(angv1v2_zplus.EqualsTol(rad_tol, 68.1d.ToRad()));

            Assert.True(angv2v1_zplus.EqualsTol(rad_tol, angv1v2_zminus));
            Assert.True(angv2v1_zplus.EqualsTol(rad_tol, 291.8d.ToRad()));
        }

        [Fact]
        public void AxisTest()
        {
            var xaxis = Vector3D.Axis(0);
            Assert.True(xaxis.EqualsTol(1e-6, Vector3D.XAxis));

            var yaxis = Vector3D.Axis(1);
            Assert.True(yaxis.EqualsTol(1e-6, Vector3D.YAxis));

            var zaxis = Vector3D.Axis(2);
            Assert.True(zaxis.EqualsTol(1e-6, Vector3D.ZAxis));
        }

        [Fact]
        public void BBoxTest()
        {
            var v = new Vector3D(1, 2, 3);
            var bbox = v.BBox(1e-6, rad_tol);
            Assert.True(bbox.Min.EqualsTol(1e-6, bbox.Max));
            Assert.True(bbox.Min.EqualsTol(1e-6, v));
        }

        [Fact]
        public void ColinearTest()
        {
            var v = new Vector3D(1, 2, 3);
            var v2 = v.ScaleAbout(Vector3D.Zero, 2);
            Assert.True(v.Colinear(1e-6, v2));
            var v3 = v2.RotateAboutZAxis(rad_tol);
            Assert.False(v.Colinear(1e-6, v3));
        }

        [Fact]
        public void ConcordantTest()
        {
            var v = new Vector3D(1, 2, 3);
            var v2 = v.ScaleAbout(Vector3D.Zero, 2);
            var v3 = v.ScaleAbout(Vector3D.Zero, .5);
            var v4 = v.ScaleAbout(Vector3D.Zero, -.5);
            Assert.True(v.Concordant(1e-6, v2));
            Assert.True(v.Concordant(1e-6, v3));
            Assert.False(v.Concordant(1e-6, v4));
        }

        [Fact]
        public void ConvertTest()
        {
            var v = new Vector3D(1, 2, 3);
            var v2 = v.Convert(MUCollection.Force.kN, MUCollection.Force.N);
            Assert.True(v2.EqualsTol(1e-6, 1e3, 2e3, 3e3));
        }

        [Fact]
        public void CrossProductTest()
        {
            var a = new Vector3D(1, 2, 3);
            var b = new Vector3D(4, 5, 6);
            var c = a.CrossProduct(b);
            Assert.True(c.Normalized().EqualsTol(1e-6, new Vector3D(-4.0825, 8.1650, -4.0825).Normalized()));
        }

        [Fact]
        public void DistanceTest()
        {
            var a = new Vector3D(1, 2, 3);
            var b = new Vector3D(4, 5, 6);
            Assert.True(a.Distance(b).EqualsTol(1e-4, 5.1962));
        }

        [Fact]
        public void Distance2DTest()
        {
            var a = new Vector3D(1, 2, 3);
            var b = new Vector3D(4, 5, 6);
            Assert.True(a.Distance2D(b).EqualsTol(1e-4, Sqrt(3 * 3 + 3 * 3)));
        }

        [Fact]
        public void DivideTest()
        {
            var v = new Vector3D(1, 2, 3);
            try
            {
                var vd = v.Divide(3);
            }
            catch (NotImplementedException)
            {
                // Geometry type vector, Divide method not implemented for Vector3D
                Assert.True(true);
            }
        }

        [Fact]
        public void DotProductTest()
        {
            var a = new Vector3D(1, 2, 3);
            var b = new Vector3D(4, 5, 6);
            var d = a.DotProduct(b);
            Assert.True(d.EqualsTol(1e-6, a.X * b.X + a.Y * b.Y + a.Z * b.Z));
        }

        [Fact]
        public void EqualsTol1Test()
        {
            var v1 = new Vector3D(1, 2, 3);
            var v2 = new Vector3D(1.1, 2.1, 3.1);
            Assert.True(v1.EqualsTol(.11, v2));
            Assert.False(v1.EqualsTol(.09, v2));
        }

        [Fact]
        public void EqualsTol2Test()
        {
            var v1 = new Vector3D(1, 2, 3);
            var v2 = new Vector3D(1.1, 2.1, 3.1);
            Assert.True(v1.EqualsTol(.11, 1.1, 2.1, 3.1));
            Assert.False(v1.EqualsTol(.09, 1.1, 2.1, 3.1));
        }

        [Fact]
        public void EqualsTol3Test()
        {
            var v1 = new Vector3D(1, 2, 3);
            var v2 = new Vector3D(1.1, 2.1, 40);
            // test only x, y
            Assert.True(v1.EqualsTol(.11, 1.1, 2.1));
            Assert.False(v1.EqualsTol(.09, 1.1, 2.1));
        }

        [Fact]
        public void From2DCoordsTest()
        {
            var v = Vector3D.From2DCoords(1, 2, 3, 4, 5, 6);
            Assert.True(v.Count() == 3);
            Assert.True(v.First().EqualsTol(1e-6, 1, 2, 0));
            Assert.True(v.Skip(1).First().EqualsTol(1e-6, 3, 4, 0));
            Assert.True(v.Skip(2).First().EqualsTol(1e-6, 5, 6, 0));
        }

        [Fact]
        public void From3DCoordsTest()
        {
            var v = Vector3D.From3DCoords(1, 2, 3, 4, 5, 6);
            Assert.True(v.Count() == 2);
            Assert.True(v.First().EqualsTol(1e-6, 1, 2, 3));
            Assert.True(v.Skip(1).First().EqualsTol(1e-6, 4, 5, 6));
        }

        [Fact]
        public void FromStringTest()
        {
            var v = Vector3D.FromString("(1.2 3.4 5.6)");
            Assert.True(v.EqualsTol(1e-6, 1.2, 3.4, 5.6));

            // 2th format with comma separated
            v = Vector3D.FromString("(1.2,3.4,5.6)");
            Assert.True(v.EqualsTol(1e-6, 1.2, 3.4, 5.6));
        }

        [Fact]
        public void FromStringArrayTest()
        {
            var v = Vector3D.FromStringArray("(1.2 3.4 5.6);(7.8 9.0 1.2)");
            Assert.True(v.Count() == 2);
            Assert.True(v.First().EqualsTol(1e-6, 1.2, 3.4, 5.6));
            Assert.True(v.Skip(1).First().EqualsTol(1e-6, 7.8, 9.0, 1.2));
        }

        [Fact]
        public void GetOrdTest()
        {
            var v = new Vector3D(1, 2, 3);
            Assert.True(v.GetOrd(0).EqualsTol(1e-6, 1));
            Assert.True(v.GetOrd(1).EqualsTol(1e-6, 2));
            Assert.True(v.GetOrd(2).EqualsTol(1e-6, 3));
        }

        [Fact]
        public void IsParallelToTest()
        {
            var v1 = new Vector3D(2.5101, 1.7754, -2.1324);
            var v2 = new Vector3D(9.0365, 6.3918, -7.6768);
            Assert.True(v1.IsParallelTo(1e-4, v2));
        }

        [Fact]
        public void IsPerpendicularTest()
        {
            var v1 = new Vector3D(2.5101, 1.7754, -2.1324);
            var v2 = new Vector3D(-9.7136, 8.0369, -4.7428);
            Assert.True(v1.IsPerpendicular(v2));
        }

        [Fact]
        public void MirrorTest()
        {
            var v = new Vector3D(1.5925, 1.5075, 3);
            var v2 = v.Mirror(new Line3D(-1.5317, 1.9230, 1.5482, 3.1248, -0.9249, -1.9787));
            Assert.True(v2.EqualsTol(1e-4, -2.316, .9075, -1.6758));
        }

        [Fact]
        public void NormalizedTest()
        {
            var v = new Vector3D(-1, 2, 4.5);
            var vn = v.Normalized();
            var l = v.Length;
            Assert.True(vn.EqualsTol(1e-6, v.X / l, v.Y / l, v.Z / l));
        }

        [Fact]
        public void Project1Test()
        {
            var cs = new CoordinateSystem3D(
                new Vector3D(6.1776, -6.3366, -5.7131), // o
                new Vector3D(-2.8849, -7.6108, -1.8691), // v1
                new Vector3D(-11.7294, 5.4484, 6.7873)); // v2

            var v = new Vector3D(1, 2, 3);
            var vp = v.Project(cs);
            Assert.True(vp.EqualsTol(1e-4, -.0151, 3.0158, .4304));
        }

        [Fact]
        public void Project2Test()
        {
            var v = new Vector3D(1.5925, 1.5075, 3);
            var v2 = v.Project(new Line3D(-1.5317, 1.9230, 1.5482, 3.1248, -0.9249, -1.9787));
            // v2 is endpoint of perp line from v to projection line
            Assert.True(v2.EqualsTol(1e-4, -.3617, 1.2075, .6621));
        }

        [Fact]
        public void Project3Test()
        {
            var v = new Vector3D(1, 2, 3);
            var v2 = new Vector3D(4.5106, 1.8377, 0);
            var vp = v.Project(v2);

            Assert.True(v2.Colinear(1e-4, vp));
            Assert.True(vp.EqualsTol(1e-4, 1.5565, .6341, 0));
        }

/*
        [Fact]
        public void Project4Test()
        {
            // Cusago, Italy : 45,448365 E, 9,034168 N ( geodetic system )
            var v = new Vector3D(9.034168, 45.448365);

            // epsg3003 : monte mario / italy zone 1 ( projected system )
            var epsg3003 = CRSCatalog.CRSList["EPSG:3003"];

            var q = v.Project(CRSCatalog.WGS84, epsg3003);
            q.EqualsTol(1e-2, 1502699.63, 5032780.63);
            // TODO
        }*/

        [Fact]
        public void Random1Test()
        {
        }

        [Fact]
        public void Random2Test()
        {

        }

        [Fact]
        public void RelTest()
        {
            var v = new Vector3D(1, 2, 3);
            Assert.True(v.Rel(new Vector3D(1, 2, 3)).EqualsTol(1e-4, Vector3D.Zero));
            Assert.True(v.Rel(new Vector3D(-1, -2, -3)).EqualsTol(1e-4, 2, 4, 6));
        }

        [Fact]
        public void RotateAboutAxis1Test()
        {
            var v = new Vector3D(1, 2, 3);
            var vr = v.RotateAboutAxis(new Line3D(6.1270, .9867, 2.3383, 5.2912, 5.8866, 1.3096), (16.5).ToRad());
            Assert.True(vr.EqualsTol(1e-4, 1.4394, 2.3514, 4.3169));
        }

        [Fact]
        public void RotateAboutAxis2Test()
        {
            var v = new Vector3D(1, 2, 3);
            var vr = v.RotateAboutAxis(new Vector3D(2.4786, 1.9027, 3), 16.5d.ToRad());
            Assert.True(vr.EqualsTol(1e-4, 1.0228, 1.6906, 3.1774));
        }

        [Fact]
        public void RotateAboutXAxisTest()
        {
            var v = new Vector3D(1, 2, 3);
            var vr = v.RotateAboutXAxis(16.5d.ToRad());
            Assert.True(vr.EqualsTol(1e-4, 1, 1.0656, 3.4445));
        }

        [Fact]
        public void RotateAboutYAxisTest()
        {
            var v = new Vector3D(1, 2, 3);
            var vr = v.RotateAboutYAxis(16.5d.ToRad());
            Assert.True(vr.EqualsTol(1e-4, 1.8109, 2, 2.5924));
        }

        [Fact]
        public void RotateAboutZAxisTest()
        {
            var v = new Vector3D(1, 2, 3);
            var vr = v.RotateAboutZAxis(16.5d.ToRad());
            Assert.True(vr.EqualsTol(1e-4, .3908, 2.2017, 3));
        }

        [Fact]
        public void RotateAsTest()
        {
            var v = new Vector3D(1, 2, 3);
            var vr = v.RotateAs(1e-4, new Vector3D(3.0258, 2.9241, 1), new Vector3D(5.561, 2.304, 2));
            Assert.True(vr.EqualsTol(1e-4, 1.4938, 1.146, 3.2335));
            vr = v.RotateAs(1e-4, new Vector3D(3.0258, 2.9241, 1), new Vector3D(5.561, 2.304, 2),
                angleFactor: 1.5);
            Assert.True(vr.EqualsTol(1e-4, 1.6205, .6586, 3.3076));
            vr = v.RotateAs(1e-4, new Vector3D(3.0258, 2.9241, 1), new Vector3D(5.561, 2.304, 2),
                angleFactor: 1.5, angleAddictional: 11.2d.ToRad());
            Assert.True(vr.EqualsTol(1e-4, 1.6602, 0.126, 3.3508));
        }

        [Fact]
        public void ScalarTest()
        {
            var v = new Vector3D(1, 2, 3);
            var s = new Vector3D(1.1, 2.2, 3.3);
            var vs = v.Scalar(s.X, s.Y, s.Z);
            Assert.True(vs.EqualsTol(1e-4, v.X * s.X, v.Y * s.Y, v.Z * s.Z));
        }

        [Fact]
        public void ScaleAbout1Test()
        {
            var v = new Vector3D(1, 2, 3);
            var vs = v.ScaleAbout(new Vector3D(3.1899, 1.3738, 1), 2.2);
            Assert.True(vs.EqualsTol(1e-4, -1.6279, 2.7514, 5.4));
        }

        [Fact]
        public void ScaleAbout2Test()
        {
            var v = new Vector3D(1, 2, 3);
            var vs = v.ScaleAbout(new Vector3D(3.1899, 1.3738, 1), new Vector3D(1.1, 2.2, 3.3));
            Assert.True(vs.EqualsTol(1e-4, .781, 2.7514, 7.6));
        }

        [Fact]
        public void SetTest()
        {
            var v = new Vector3D(1, 2, 3);
            var vv = v.Set(OrdIdx.X, 1.1);
            Assert.True(vv.EqualsTol(1e-4, 1.1, v.Y, v.Z));
            vv = v.Set(OrdIdx.Y, 2.2);
            Assert.True(vv.EqualsTol(1e-4, v.X, 2.2, v.Z));
            vv = v.Set(OrdIdx.Z, 3.3);
            Assert.True(vv.EqualsTol(1e-4, v.X, v.Y, 3.3));
        }

        [Fact]
        public void StringRepresentationTest()
        {
            var v = new Vector3D(1.2, 3.4, 5.6);
            var vs = v.StringRepresentation();
            Assert.True(vs == "(1.2, 3.4, 5.6)");
        }

        [Fact]
        public void ToString1Test()
        {
            var v = new Vector3D(1.1234, 2.2345, 3.3456);
            var vs = v.ToString();
            Assert.True(vs == "(1.123, 2.235, 3.346)");
        }

        [Fact]
        public void ToString2Test()
        {
            var v = new Vector3D(1.1234, 2.2345, 3.3456);
            var vs = v.ToString(4);
            Assert.True(vs == "(1.1234, 2.2345, 3.3456)");
        }

        [Fact]
        public void ToString3Test()
        {
            var v = new Vector3D(1.1234, 2.2345, 3.3456);
            var vs = v.ToString(1e-2);
            Assert.True(vs == "(1.12, 2.23, 3.35)");
        }

        [Fact]
        public void ToUCSTest()
        {
            var v = new Vector3D(1, 2, 3);
            var cs = new CoordinateSystem3D(
                new Vector3D(1.8129, 1.8060, .2726), // origin
                new Vector3D(1.8404, 2.0375, 1.3964), // v1
                new Vector3D(2.8872, .2899, 1.3186)); // v2
            var v_ = v.ToUCS(cs);
            Assert.True(v_.EqualsTol(1e-4, .8791, -.4619, -2.6742));
        }

        [Fact]
        public void ToWCSTest()
        {
            var v_ = new Vector3D(.8791, -.4619, -2.6742);
            var cs = new CoordinateSystem3D(
                new Vector3D(1.8129, 1.8060, .2726), // origin
                new Vector3D(1.8404, 2.0375, 1.3964), // v1
                new Vector3D(2.8872, .2899, 1.3186)); // v2
            var v = v_.ToWCS(cs);
            Assert.True(v.EqualsTol(1e-4, 1, 2, 3));
        }

        [Fact]
        public void Vector3D1Test()
        {
            var v = new Vector3D();
            Assert.True(v.EqualsTol(1e-4, Vector3D.Zero));
        }

        [Fact]
        public void Vector3D2Test()
        {
            var v = new Vector3D(new[] { 1.2, 3.4 });
            Assert.True(v.EqualsTol(1e-4, 1.2, 3.4, 0));

            v = new Vector3D(new[] { 1.2, 3.4, 5.6 });
            Assert.True(v.EqualsTol(1e-4, 1.2, 3.4, 5.6));
        }

        [Fact]
        public void Vector3D3Test()
        {
            var v = new Vector3D(1.2, 3.4);
            Assert.True(v.EqualsTol(1e-4, 1.2, 3.4, 0));
        }

        [Fact]
        public void Vector3D4Test()
        {
            var v = new Vector3D(1.2, 3.4, 5.6);
            Assert.True(v.EqualsTol(1e-4, 1.2, 3.4, 5.6));
        }

        [Fact]
        public void OperatorSub1Test()
        {
            var v = new Vector3D(1, 2, 3);
            var iv = -v;
            Assert.True(iv.EqualsTol(1e-4, -v.X, -v.Y, -v.Z));
        }

        [Fact]
        public void OperatorSub2Test()
        {
            var v1 = new Vector3D(1, 2, 3);
            var v2 = new Vector3D(4, 5, 6);
            var vd = v1 - v2;
            Assert.True(vd.EqualsTol(1e-4, v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z));
        }

        [Fact]
        public void OperatorScalarMul1Test()
        {
            var v = new Vector3D(1.2, 3.4, 5.6);
            var s = 7.8;
            var vs = s * v;
            Assert.True(vs.EqualsTol(1e-4, v.X * s, v.Y * s, v.Z * s));
        }

        [Fact]
        public void OperatorScalarMul2Test()
        {
            var v = new Vector3D(1.2, 3.4, 5.6);
            var s = 7.8;
            var vs = v * s;
            Assert.True(vs.EqualsTol(1e-4, v.X * s, v.Y * s, v.Z * s));
        }

        [Fact]
        public void OperatorMulTest()
        {
            var v1 = new Vector3D(1.2, 3.4, 5.6);
            var v2 = new Vector3D(7.8, 9.0, 1.2);
            var vs = v1 * v2;
            Assert.True(vs.EqualsTol(1e-4, v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z));
        }

        [Fact]
        public void OperatorDivide1Test()
        {
            var v = new Vector3D(1.2, 3.4, 5.6);
            var s = 7.8;
            var vs = s / v;
            Assert.True(vs.EqualsTol(1e-4, s / v.X, s / v.Y, s / v.Z));
        }

        [Fact]
        public void OperatorDivide2Test()
        {
            var v = new Vector3D(1.2, 3.4, 5.6);
            var s = 7.8;
            var vs = v / s;
            Assert.True(vs.EqualsTol(1e-4, v.X / s, v.Y / s, v.Z / s));
        }

        [Fact]
        public void OperatorSumTest()
        {
            var v1 = new Vector3D(1.2, 3.4, 5.6);
            var v2 = new Vector3D(7.8, 9.0, 1.2);
            var vs1 = v1 + v2;
            var vs2 = v2 + v1;
            Assert.True(vs1.EqualsTol(1e-4, vs2));
            Assert.True(vs1.EqualsTol(1e-4, v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z));
        }

    }

}

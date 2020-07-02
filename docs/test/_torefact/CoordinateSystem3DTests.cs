using Xunit;

namespace SearchAThing.Sci.Tests
{
    public class CoordinateSystem3DTests
    {

        public CoordinateSystem3DTests()
        {
        }

        /// <summary>
        /// CoordinateSystem3DTest_001.dxf
        /// </summary>
        [Fact]
        public void CoordinateSystem3DTest_001()
        {
            var cs1cad = new CoordinateSystem3D(new Vector3D(-1, 77, .75),
                new Vector3D(0, 0, 1),
                new Vector3D(.619, -.785, 0),
                new Vector3D(.785, .619, 0));

            var rotationAxis = new Line3D(new Vector3D(.111, .652, 0), Vector3D.ZAxis, Line3DConstructMode.PointAndVector);

            var cs2 = cs1cad.Rotate(rotationAxis, 74.1566195.ToRad());
            var cs2cad = new CoordinateSystem3D(new Vector3D(-73.639, 20.427, .75),
                new Vector3D(0, 0, 1),
                new Vector3D(.924, .381, 0),
                new Vector3D(-.381, .924, 0));

            Assert.True(cs2.Equals(1e-3, cs2cad));
        }

        /// <summary>
        /// CoordinateSystem3DTest_002.dxf
        /// </summary>
        [Fact]
        public void CoordinateSystem3DTest_002()
        {
            var p = new Vector3D(53.0147, 34.5182, 20.1);

            var o = new Vector3D(15.3106, 22.97, 0);
            var v1 = new Vector3D(10.3859, 3.3294, 30);
            var v2 = new Vector3D(2.3515, 14.101, 0);

            var cs = new CoordinateSystem3D(o, v1, v2);

            var u = p.ToUCS(cs);
            Assert.True(u.EqualsTol(1e-4, 32.3623, 12.6875, -27.3984));
            Assert.True(u.ToWCS(cs).EqualsTol(1e-4, p));

            Assert.True(p.ToUCS(cs).EqualsTol(1e-4, cs.ToUCS(p)));
            Assert.True(cs.ToWCS(u).EqualsTol(1e-4, u.ToWCS(cs)));
        }

        /// <summary>
        /// CoordinateSystem3DTest_003.dxf
        /// </summary>
        [Fact]
        public void CoordinateSystem3DTest_003()
        {
            var cs1cad = new CoordinateSystem3D(new Vector3D(-1, 77, .75),
                new Vector3D(0, 0, 1),
                new Vector3D(.619, -.785, 0),
                new Vector3D(.785, .619, 0));

            var cs2cad = new CoordinateSystem3D(new Vector3D(-20.74, 55.485, 0),
                new Vector3D(0, 0, 1),
                new Vector3D(.619, -.785, 0),
                new Vector3D(.785, .619, 0));

            var delta = new Vector3D(-19.74, -21.515, -0.75);
            Assert.True(cs1cad.Move(delta).Equals(1e-3, cs2cad));
        }

        /// <summary>
        /// CoordinateSystem3DTest_004.dxf
        /// </summary>
        [Fact]
        public void CoordinateSystem3DTest_004()
        {
            var cs1cad = new CoordinateSystem3D(new Vector3D(-1, 77, .75),
                new Vector3D(0, 0, 1),
                new Vector3D(.619, -.785, 0),
                new Vector3D(.785, .619, 0));

            var cs2cad = new CoordinateSystem3D(new Vector3D(-1, 77, .75),
                new Vector3D(-0.649, .409, .642),
                new Vector3D(-.108, -.884, .454),
                new Vector3D(.753, .225, .618));

            var vaxis = new Vector3D(-.311, -1.15, -.75);
            Assert.True(cs1cad.Rotate(vaxis, 60d.ToRad()).Equals(1e-3, cs2cad));

            var pt = new Vector3D(-1.379, 76.762, 1.298);
            Assert.True(cs2cad.Contains(1e-3, pt) && !cs1cad.Contains(1e-3, pt));
        }

        /// <summary>
        /// CoordinateSystem3DTest_005.dxf
        /// </summary>
        [Fact]
        public void CoordinateSystem3DTest_005()
        {
            var cs1o = new Vector3D("X = -2.67223215 Y = -94.22126913 Z = -293.08306335");
            var cs1x = new Line3D(cs1o, new Vector3D("X = -221.37919777 Y = 144.86810067 Z = -168.96320246"));
            var cs1y = new Line3D(cs1o, new Vector3D("X = 192.45964896 Y = 5.43267925 Z = -141.20969062"));
            var cs1 = new CoordinateSystem3D(cs1o, cs1x.V, cs1y.V);

            var cs2o = new Vector3D("X = -187.42845697 Y = 128.44203407 Z = 25.92565607");
            var cs2x = new Line3D(cs2o, new Vector3D("X = -9.40307509 Y = 246.81618785 Z = 25.92565607"));
            var cs2y = new Line3D(cs2o, new Vector3D("X = -187.42845697 Y = 128.44203407 Z = 224.4413331"));
            var cs2 = new CoordinateSystem3D(cs2o, cs2x.V, cs2y.V);

            var i12Line = cs1.Intersect(1e-4, cs2);
            var i21Line = cs2.Intersect(1e-4, cs1);

            Assert.True(i12Line.LineContainsPoint(1e-4, i21Line.From));
            Assert.True(i12Line.LineContainsPoint(1e-4, i21Line.To));
            Assert.True(i12Line.LineContainsPoint(1e-4, new Vector3D("X = -187.42845697 Y = 128.44203407 Z = -170.87092273")));
            Assert.True(i12Line.LineContainsPoint(1e-4, new Vector3D("X = 29.37373838 Y = 272.59999706 Z = 25.92565607")));
        }

    }

}
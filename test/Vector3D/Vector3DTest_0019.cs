using Xunit;
using System.Linq;
using static System.Math;
using System;
using SearchAThing.Util;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {

        [Fact]
        public void Vector3DTest_0019()
        {
            var tol = 1e-8;

            var v1 = new Vector3D("X = 220.22063137 Y = 217.58532235 Z = 30.72201149");
            var v2 = new Vector3D("X = 41.2984487 Y = 335.41147265 Z = 65.72177141");
            var v1v2angle = v1.AngleRad(tol, v2);

            var cp = v1.CrossProduct(v2);

            // length of cross product is |a| |b| sin(alfa)            
            Assert.True(cp.Length.EqualsTol(tol, v1.Length * v2.Length * Sin(v1v2angle)));

            // and is perpendicular to component vectors
            Assert.True(cp.IsPerpendicular(v1));
            Assert.True(cp.IsPerpendicular(v2));

            // test v1 x v2 = cp meet right hand rule
            var cpdir = new Vector3D("X = 15.98231081 Y = -52.81807432 Z = 259.51436001");
            Assert.True(cp.ConcordantColinear(tol, cpdir));

        }
    }
}
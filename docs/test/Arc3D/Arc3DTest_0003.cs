using Xunit;
using System.Linq;
using System;
using static System.Math;

namespace SearchAThing.Sci.Tests
{
    public partial class Arc3DTests
    {

        [Fact]
        public void Arc3DTest_0003()
        {
            var tolBuildArc = 1e-1;
            var tol = 1e-7;

            var arcFrom = new Vector3D(0.000000000000003, 12.72, 0);
            var arcMiddle = new Vector3D(1.90117406287241, 17.0988259371276, 0);
            var arcTo = new Vector3D(6.28, 19, 0);

            var arc = new Arc3D(
                tolBuildArc,
                p1: arcFrom,
                p2: arcMiddle,
                p3: arcTo);

            arc.From.AssertEqualsTol(tol, arcFrom);
            arc.MidPoint.AssertEqualsTol(tol, arcMiddle);
            arc.To.AssertEqualsTol(tol, arcTo);
        }
    }

}
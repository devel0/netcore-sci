using Xunit;
using System.Linq;
using System;
using AngouriMath;
using SearchAThing;
using AngouriMath.Extensions;

namespace SearchAThing.Sci.Tests
{
    public partial class MathTests
    {

        [Fact]
        public void MathTest_0001()
        {
            var accelBase = "1-cos(t_r)".Substitute("t_r", "(t_r/d*(2*pi))");
            var accelCoeff = $"s_d / ({accelBase.Integrate("t_r").Substitute("t_r", "d")})";
            var a_tr = $"({accelCoeff}) * ({accelBase})".Simplify();
            var a_max = $"{a_tr}".Substitute("t_r", "d/2").Simplified;
            var d = $"a_max={a_max}".Solve("d").DirectChildren.First().Simplified;
            var s_tr = $"s_0+{a_tr.Integrate("t_r")}".Simplify();
            var x_tr = $"x_0+{s_tr.Integrate("t_r") - s_tr.Integrate("t_r").Substitute("t_r", 0)}".Simplify();
            var s_d = $"x_d={x_tr}".Substitute("t_r", "d").Solve("s_d").DirectChildren.First().Simplified;
            Entity x_d = $"{x_tr.Substitute("t_r", "d")}";

            var x_d_simp1 = x_d.Simplify();
            var x_d_simp2 = x_d.VeryExpensiveSimplify(2);
            
            // this test could fail in future if Simplify improves
            Assert.True(x_d_simp1.ToString() == "d * (-1/4 * s_d / pi ^ 2 + 1/4 * s_d / pi ^ 2 + s_0 + 1/2 * s_d) + x_0");

            // this test should not fail
            Assert.True(x_d_simp2.ToString() == "d * (s_0 + 1/2 * s_d) + x_0");
        }

    }
}
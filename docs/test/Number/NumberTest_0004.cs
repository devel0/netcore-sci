using Xunit;
using System.Linq;
using System;
using static System.Math;
using static SearchAThing.SciToolkit;
using System.Collections.Generic;

namespace SearchAThing.Sci.Tests
{
    public partial class NumberTests
    {

        [Fact]
        public void NumberTest_0004()
        {
            List<double> xset = new List<double>();
            List<double> yset = new List<double>();

            var K = 40;
            var x = 0d;
            for (int i = 0; i < K; ++i)
            {
                xset.Add(x);
                yset.Add(Sin(x));
                x += PI / 2 / K;
            }

            var ipol = LinearSplineInterpolate(xset, yset);

            x = PI / 2 / K / 2;
            for (int i = 0; i < K - 1; ++i)
            {
                x += PI / 2 / K;

                var expected = Sin(x);
                var interpolated = ipol.Interpolate(x);
                var diff = Abs(expected-interpolated);

                System.Console.WriteLine($"expected:{expected} interpolated:{interpolated} diff:{diff}");

                Assert.True(diff < 6e-4);
            }
        }

    }
}
using Xunit;
using System.Linq;
using System;

namespace SearchAThing.Sci.Tests
{
    public partial class NumberTests
    {

        public struct NumberTest_0002SetNfo
        {
            public double a, b, s;
        }

        [Fact]
        public void NumberTest_0002()
        {
            var lst = new NumberTest_0002SetNfo[]
            {
                new NumberTest_0002SetNfo { a = -1, b = 1, s = 1 },
                new NumberTest_0002SetNfo { a = -5, b = 5, s = 5 },

                new NumberTest_0002SetNfo { a = -1e-10, b = 1e-10, s = 1e-10 },
                new NumberTest_0002SetNfo { a = -1e-20, b = 1e-20, s = 1e-20 },

                new NumberTest_0002SetNfo { a = -1, b = 0, s = 0.5 },
                new NumberTest_0002SetNfo { a = 0, b = 1, s = 0.5 },

                new NumberTest_0002SetNfo { a = 0.5, b = 1.6, s = 2.2 },
                new NumberTest_0002SetNfo { a = 0.5+1e3, b = 1.6+1e3, s = 0.0010994502748625915 },
            };

            foreach (var t in lst)
            {
                Assert.True(t.a.Similarity(t.b) == t.s);
            }

        }

    }
}
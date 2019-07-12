using Xunit;
using System.Linq;
using static System.Math;
using System;
using SearchAThing.Util;
using System.Threading;

namespace SearchAThing.Sci.Tests
{
    public class PythonTests : IDisposable
    {
        PythonPipe pipe = null;
        PythonPipe sympyPipe = null;

        bool has_sympy_library = true;

        public PythonTests()
        {
            pipe = new PythonPipe();
            sympyPipe = new PythonPipe("from sympy import *", (s) => System.Diagnostics.Debug.WriteLine(s));
            try
            {
                var strwr = new StringWrapper();
                strwr.str = "x = symbols('x')";
                sympyPipe.Exec(strwr);
            }
            catch
            {
                has_sympy_library = false;
            }
        }

        public void Dispose()
        {
            sympyPipe.Dispose();
            pipe.Dispose();
        }

        [Fact]
        public void Test0()
        {
            var strwr = new StringWrapper();
            strwr.str = "print(1+2)";
            var res = pipe.Exec(strwr).str;
            var reslines = res.Lines().ToArray();
            Assert.True(reslines.Length == 1 && reslines[0] == "3");
        }

        [Fact]
        public void Test1()
        {
            var strwr = new StringWrapper();
            strwr.str = "print(1+2)\r\nprint(3+5)";
            var res = pipe.Exec(strwr).str;
            var reslines = res.Lines().ToArray();
            Assert.True(reslines.Length == 2 && reslines[0] == "3" && reslines[1] == "8");
        }

        [SkippableFact]
        public void Test2()
        {            
            Skip.IfNot(has_sympy_library);

            var strwr = new StringWrapper();
            strwr.str = @"x = symbols('x')
print(integrate(x**2 * cos(x), x))
print(integrate(x**2 * cos(x), (x, 0, pi/2)))";
            var res = sympyPipe.Exec(strwr).str;
            var reslines = res.Lines().ToArray();
            Assert.True(reslines.Length == 2 && reslines[0] == "x**2*sin(x) + 2*x*cos(x) - 2*sin(x)" && reslines[1] == "-2 + pi**2/4");
        }

    }

}
using Xunit;
using System.Linq;
using System;

namespace SearchAThing.Sci.Tests
{
    public class PythonTests_torefact : IDisposable
    {
        PythonPipe pipe;
        PythonPipe sympyPipe;

        bool has_sympy_library = true;

        public PythonTests_torefact()
        {
            pipe = new PythonPipe();
            sympyPipe = new PythonPipe("from sympy import *", (s) => System.Diagnostics.Debug.WriteLine(s));
            try
            {
                var strwr = new StringWrapper("x = symbols('x')");                
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
        public async void Test0()
        {
            var strwr = new StringWrapper("print(1+2)");            
            var res = (await pipe.Exec(strwr)).str;
            var reslines = res.Lines().ToArray();
            Assert.True(reslines.Length == 1 && reslines[0] == "3");
        }

        [Fact]
        public async void Test1()
        {
            var strwr = new StringWrapper("print(1+2)\r\nprint(3+5)");            
            var res = (await pipe.Exec(strwr)).str;
            var reslines = res.Lines().ToArray();
            Assert.True(reslines.Length == 2 && reslines[0] == "3" && reslines[1] == "8");
        }

        [SkippableFact]
        public async void Test2()
        {
            Skip.IfNot(has_sympy_library);

            var strwr = new StringWrapper(@"x = symbols('x')
print(integrate(x**2 * cos(x), x))
print(integrate(x**2 * cos(x), (x, 0, pi/2)))");            
            var res = (await sympyPipe.Exec(strwr)).str;
            var reslines = res.Lines().ToArray();
            Assert.True(reslines.Length == 2 && reslines[0] == "x**2*sin(x) + 2*x*cos(x) - 2*sin(x)" && reslines[1] == "-2 + pi**2/4");
        }

    }

}
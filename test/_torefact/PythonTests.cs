using Xunit;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace SearchAThing.Sci.Tests
{
    public class PythonTests_torefact : IDisposable
    {
        PythonPipe pipe;             

        public PythonTests_torefact()
        {
            pipe = new PythonPipe(new PythonPipeOptions
            {
                Debug = (str) =>
                {
                    // System.Console.WriteLine($"pydbg> [{str}]");
                }
            });
        }

        public void Dispose()
        {            
            pipe.Dispose();
        }

        [Fact]
        public async void Test0()
        {
            // System.Console.WriteLine("PYTHON TEST");            
            var res = (await pipe.Exec("print(1+2)"));
            var reslines = res.Lines().ToArray();
            Assert.True(reslines.Length == 1 && reslines[0] == "3");
        }

        [Fact]
        public async void Test1()
        {            
            var res = (await pipe.Exec("print(1+2)\r\nprint(3+5)"));
            var reslines = res.Lines().ToArray();
            Assert.True(reslines.Length == 2 && reslines[0] == "3" && reslines[1] == "8");
        }

        [SkippableFact]
        public async void Test2()
        {
            var skip = false;
            try
            {
                var sympyPipeTest = new PythonPipe();
                await sympyPipeTest.Exec("from sympy import *");
            }
            catch
            {
                skip = true;
            }

            Skip.If(skip);
            
            var res = (await pipe.Exec(@"from sympy import *
x = symbols('x')
print(integrate(x**2 * cos(x), x))
print(integrate(x**2 * cos(x), (x, 0, pi/2)))"));
            var reslines = res.Lines().ToArray();
            Assert.True(reslines.Length == 2 && reslines[0] == "x**2*sin(x) + 2*x*cos(x) - 2*sin(x)" && reslines[1] == "-2 + pi**2/4");
        }

    }

}
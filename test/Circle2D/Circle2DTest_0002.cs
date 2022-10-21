using Xunit;
using System.Linq;

using static SearchAThing.SciToolkit;
using SearchAThing;
namespace SearchAThing.Sci.Tests
{
    public partial class ToolkitTests
    {

        [Fact]
        public void Circle2DTest_0002()
        {
            var tol = 1e-4;
            {
                var q = SciToolkit.Circle2D(tol,
                    11.66190379,
                    new Vector3D(34.77919275, 20.45073069),
                    new Vector3D(15.3953039, 23.87141696));

                Assert.True(q.Length == 2);

                q[0].AssertEqualsTol(tol, 24, 16, 0);
                q[1].AssertEqualsTol(tol, 26.17449664, 28.32214765, 0);
            }

            {
                var q = SciToolkit.Circle2D(tol,
                    11.66190379,
                    new Vector3D(15.72131998, 24.21361413),
                    new Vector3D(32.27868002, 7.78638587));

                Assert.True(q.Length == 1);

                q[0].AssertEqualsTol(tol, 24, 16, 0);
            }

        }
    }
}
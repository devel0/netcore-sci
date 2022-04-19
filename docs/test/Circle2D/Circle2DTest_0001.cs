using Xunit;
using System.Linq;

using static SearchAThing.SciToolkit;

namespace SearchAThing.Sci.Tests
{
    public partial class ToolkitTests
    {

        [Fact]
        public void Circle2DTest_0001()
        {
            var res = CirclesOuterTangent(1e-1, new Vector3D(75, 175), 12.5, new Vector3D(175, 25), 7);

            Assert.True(res != null);

            if (res == null)
            {
                Assert.True(false);
            }
            else
            {
                Assert.True(res.pa1.EqualsTol(1e-3, 85.401, 181.934));
                Assert.True(res.pa2.EqualsTol(1e-3, 64.599, 168.066));
                Assert.True(res.pb1.EqualsTol(1e-3, 180.824, 28.883));
                Assert.True(res.pb2.EqualsTol(1e-3, 169.176, 21.117));
            }

        }
    }
}
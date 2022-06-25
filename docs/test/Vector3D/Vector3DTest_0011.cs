using Xunit;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {
        
        [Fact]
        public void Vector3DTest_0011()
        {                                    
            Assert.True(Vector3D.Zero.IsZeroLength);
        }
    }
}
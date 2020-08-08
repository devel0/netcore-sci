using System.Numerics;
using Xunit;
using SearchAThing;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {

        [Fact]
        public void Matrix4x4Test_0001()
        {
            var angles = new Vector3D(55.6f, 13.5f, 6.5f).ToRad();

            var m = Matrix4x4.Identity *
                Matrix4x4.CreateRotationX((float)angles.X) *
                Matrix4x4.CreateRotationY((float)angles.Y) *
                Matrix4x4.CreateRotationZ((float)angles.Z);

            var q = (Vector3D)m.ToEulerAngles();

            Assert.True(q.EqualsTol(1e-6, angles));

        }

    }
}
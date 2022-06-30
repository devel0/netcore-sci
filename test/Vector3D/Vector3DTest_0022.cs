using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Xunit;

namespace SearchAThing.Sci.Tests
{
    public partial class Vector3DTests
    {

        [Fact]
        public void Vector3DTest_0022()
        {
            var tol = 1e-1;

            var pts = JsonConvert.DeserializeObject<List<Vector3D>>(
                File.ReadAllText("Vector3D/Vector3DTest_0022-pts.json"), SciToolkit.SciJsonSettings);

            var q = pts.BestFittingPlane(tol);

            // TODO: q should (6.28, 19, 0) X:(0,0,1), Y(-1,0,0), Z(0,-1,0)

            ;
        }
    }
}
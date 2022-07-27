using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Xunit;

using static SearchAThing.SciToolkit;

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

            q.CS.Origin.AssertEqualsTol(tol, 6.28, 19, 0);
            q.CS.BaseX.AssertEqualsTol(NormalizedLengthTolerance, 1, 0, 0);
            q.CS.BaseY.AssertEqualsTol(NormalizedLengthTolerance, 0, 0, 1);
            q.CS.BaseZ.AssertEqualsTol(NormalizedLengthTolerance, 0, -1, 0);
        }
    }
}
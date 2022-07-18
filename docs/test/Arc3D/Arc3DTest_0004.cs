using Xunit;
using System.Linq;
using System;
using static System.Math;
using Newtonsoft.Json;
using System.IO;

using static SearchAThing.SciToolkit;

namespace SearchAThing.Sci.Tests
{
    public partial class Arc3DTests
    {

        [Fact]
        public void Arc3DTest_0004()
        {
            var tol = 0.1;

            Edge? arc = JsonConvert.DeserializeObject<Arc3D>(File.ReadAllText("Arc3D/Arc3DTest_0004.json"), SciJsonSettings);

            var res = arc.Split(tol, new[] { new Vector3D(15, 12.719999999999814, -2) }).ToList();

            Assert.True(res.Count == 1);
        }

    }

}
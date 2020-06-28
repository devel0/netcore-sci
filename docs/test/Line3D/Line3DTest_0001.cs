using Xunit;
using System.Linq;
using static System.Math;
using System;
using SearchAThing;

namespace SearchAThing.Sci.Tests
{
  public partial class Vector3DTests
  {

    [Fact]
    public void Line3DTest_0001()
    {
      var tol = 1e-8;
      // 1e-6 deg tolerance for angle comparision
      var radTol = (1e-6 * MUCollection.PlaneAngle.deg).ConvertTo(MUCollection.PlaneAngle.rad).Value;

      var l1 = new Line3D(
        new Vector3D("X = -77.94731049 Y = 92.20123462 Z = 51.4961973"),
        new Vector3D("X = 423.87037202 Y = 153.83733838 Z = -0.22263464"));

      var l2 = new Line3D(
        new Vector3D("X = 72.77627807 Y = 35.03080659 Z = 10"),
        new Vector3D("X = 163.78240786 Y = 236.91916385 Z = 164.44414363"));

      var perpSeg = l1.ApparentIntersect(l2);
      
      Assert.True(l1.V.AngleRad(tol, perpSeg.V).EqualsTol(radTol, PI / 2));
      Assert.True(l2.V.AngleRad(tol, perpSeg.V).EqualsTol(radTol, PI / 2));

      Assert.True(perpSeg.From.EqualsTol(tol, new Vector3D("X = 95.06578762 Y = 113.451688 Z = 33.66494971")));
      Assert.True(perpSeg.To.EqualsTol(tol, new Vector3D("X = 99.63797128 Y = 94.62089275 Z = 55.58628319")));
    }
  }
}
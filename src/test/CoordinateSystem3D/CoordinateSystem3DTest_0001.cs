namespace SearchAThing.Sci.Tests;

public partial class CoordinateSystem3DTests
{

    [Fact]
    public void CoordinateSystem3D_0001()
    {
        var tol = NormalizedLengthTolerance;

        var cs = CoordinateSystem3D.WCS;

        var csFlipX = cs.FlipX();
        csFlipX.BaseX.AssertEqualsTol(tol, -cs.BaseX);
        csFlipX.BaseY.AssertEqualsTol(tol, cs.BaseY);
        csFlipX.BaseZ.AssertEqualsTol(tol, -cs.BaseZ);

        var csFlipY = cs.FlipY();
        csFlipY.BaseX.AssertEqualsTol(tol, cs.BaseX);
        csFlipY.BaseY.AssertEqualsTol(tol, -cs.BaseY);
        csFlipY.BaseZ.AssertEqualsTol(tol, -cs.BaseZ);

        var csFlipZ = cs.FlipZ();
        csFlipZ.BaseX.AssertEqualsTol(tol, cs.BaseX);
        csFlipZ.BaseY.AssertEqualsTol(tol, -cs.BaseY);
        csFlipZ.BaseZ.AssertEqualsTol(tol, -cs.BaseZ);
    }

}
namespace SearchAThing.Sci.Tests;

public partial class Arc3DTests
{

    [Fact]
    public void Arc3DTest_0002()
    {
        var dxf = netDxf.DxfDocument.Load(
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Arc3D/Arc3DTest_0002.dxf"));

        var tol = 1e-8;
        var precision = Abs(tol.Magnitude());

        var arc = dxf.Entities.Arcs.First().ToArc3D();

        arc.Length.AssertEqualsTol(tol, 3.37822217);

        arc.CircularSectorArea.AssertEqualsTol(tol, 7.20094727);

        arc.ChordTriangleArea.AssertEqualsTol(tol, 6.4706385);

        arc.SegmentArea.AssertEqualsTol(tol, 0.73030876);
    }
}
namespace SearchAThing.Sci.Tests;

public partial class Arc3DTests
{

    [Fact]
    public void Arc3DTest_0007()
    {
        DxfDocument? outdxf = null;
        //outdxf = new DxfDocument();

        var tol = 1e-3;

        var p1 = new Vector3D(15, 12.28);
        var p2 = new Vector3D(15.003722011278999, 12.499999999999998);
        var p3 = new Vector3D(15, 12.72);

        var arc = new Arc3D(tol, p1, p2, p3);
        outdxf?.AddEntity(arc.DxfEntity);
        outdxf?.AddEntity(p1.DxfEntity);
        outdxf?.AddEntity(p2.DxfEntity);
        outdxf?.AddEntity(p3.DxfEntity);

        arc.Length.AssertEqualsTol(1e-8, 0.44008396);
        arc.CircularSectorArea.AssertEqualsTol(1e-8, 1.43109185);

        if (outdxf != null)
        {
            outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
            outdxf.Viewport.ShowGrid = false;
            outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
        }
    }
}
namespace SearchAThing.Sci.Tests;

public partial class LoopTests
{

    [Fact]
    public void LoopTest_0007()
    {
        var dxf = netDxf.DxfDocument.Load(
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0007.dxf"));

        DxfDocument? outdxf = null;
        //outdxf = new DxfDocument();

        var tol = 0.1;//1e-8;

        var lworig = (Polyline2D)dxf.Entities.Polylines2D.First().Clone();
        lworig.SetColor(AciColor.Blue);
        outdxf?.AddEntity(lworig);

        var loop = dxf.Entities.Polylines2D.First().ToLoop(tol);
        var relw = loop.DxfEntity(tol);
        relw.SetColor(AciColor.Red);

        outdxf?.AddEntity(relw);

        Assert.True(lworig.Vertexes.Count == relw.Vertexes.Count);
        for (int i = 0; i < lworig.Vertexes.Count; ++i)
        {
            ((Vector3D)lworig.Vertexes[i].Position)
                .AssertEqualsTol(tol, relw.Vertexes[i].Position);
        }

        if (outdxf != null)
        {
            outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
            outdxf.Viewport.ShowGrid = false;
            outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
        }

    }

}
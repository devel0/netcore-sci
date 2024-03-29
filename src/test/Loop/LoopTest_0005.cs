namespace SearchAThing.Sci.Tests;

public partial class LoopTests
{

    [Fact]
    public void LoopTest_0005()
    {
        var dxf = netDxf.DxfDocument.Load(
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Loop/LoopTest_0005.dxf"));

        DxfDocument? outdxf = null;
        // outdxf = new DxfDocument();

        DxfDocument? outdxf2 = null;
        // outdxf2 = new DxfDocument();

        var tol = 1e-8;

        var faceGreen = dxf.Entities.Polylines2D.First(w => w.Color.Index == AciColor.Green.Index).ToLoop(tol).ToFace();
        var faceYellow = dxf.Entities.Polylines2D.First(w => w.Color.Index == AciColor.Yellow.Index).ToLoop(tol).ToFace();

        outdxf?.AddEntity(faceGreen.Loops[0].DxfEntity(tol).Act(x => x.SetColor(AciColor.Green)));
        outdxf?.AddEntity(faceYellow.Loops[0].DxfEntity(tol).Act(x => x.SetColor(AciColor.Yellow)));

        var res = faceYellow.Boolean(tol, faceGreen).ToList();

        Assert.True(res.Count == 0);

        if (outdxf != null)
        {
            outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
            outdxf.Viewport.ShowGrid = false;
            outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
        }

        if (outdxf2 != null)
        {
            outdxf2.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
            outdxf2.Viewport.ShowGrid = false;
            outdxf2.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out2.dxf"));
        }
    }

}
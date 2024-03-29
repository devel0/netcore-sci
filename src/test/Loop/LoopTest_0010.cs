namespace SearchAThing.Sci.Tests;

public partial class LoopTests
{

    [Fact]
    public void LoopTest_0010()
    {
        DxfDocument? outdxf = null;
        //outdxf = new DxfDocument();

        var tol = 0.1;//1e-8;

        var face1 = JsonConvert.DeserializeObject<Loop>(
            File.ReadAllText("Loop/LoopTest_0010-loop1.json"), SciJsonSettings)!.ToFace();

        var face2 = JsonConvert.DeserializeObject<Loop>(
            File.ReadAllText("Loop/LoopTest_0010-loop2.json"), SciJsonSettings)!.ToFace();

        var faceYellow = face1;
        var faceGreen = face2;

        var ints = faceYellow.Boolean(tol, faceGreen).ToList();

        outdxf?.AddEntity(faceGreen.Loops[0].DxfEntity(tol).Act(x => x.SetColor(AciColor.Green)));
        outdxf?.AddEntity(faceYellow.Loops[0].DxfEntity(tol).Act(x => x.SetColor(AciColor.Yellow)));

        Assert.True(ints.Count == 0);
        //outdxf?.AddEntity(ints[0].DxfEntity(tol).Set(x => x.SetColor(AciColor.Red))); 

        if (outdxf != null)
        {
            outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
            outdxf.Viewport.ShowGrid = false;
            outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
        }
    }

}
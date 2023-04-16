namespace SearchAThing.Sci.Tests;

public partial class LoopTests
{

    [Fact]
    public void LoopTest_0017()
    {
        DxfDocument? outdxf = null;
        // outdxf = new DxfDocument();

        DxfDocument? outdxf2 = null;
        // outdxf2 = new DxfDocument();

        var tol = 1e-8;

        var face1 = JsonConvert.DeserializeObject<Loop>(
            File.ReadAllText("Loop/LoopTest_0017-loop1.json"), SciJsonSettings)!.ToFace();

        var face2 = JsonConvert.DeserializeObject<Loop>(
            File.ReadAllText("Loop/LoopTest_0017-loop2.json"), SciJsonSettings)!.ToFace();

        var faceYellow = face1;
        var faceGreen = face2;
        // Difference
        var ints = faceYellow.Boolean(tol, faceGreen, Face.BooleanMode.Intersect, outdxf2).ToList();

        outdxf?.AddEntity(faceGreen.Loops[0].DxfEntity(tol).Act(x => x.SetColor(AciColor.Green)));
        outdxf?.AddEntity(faceYellow.Loops[0].DxfEntity(tol).Act(x => x.SetColor(AciColor.Yellow)));
        outdxf?.AddEntity(ints[0].Loops[0].DxfEntity(tol).Act(x => x.SetColor(AciColor.Red)));

        Assert.True(ints.Count == 1 && ints[0].Loops.Count == 1);
        ints[0].Loops[0].Area.AssertEqualsTol(tol, 16.68099798);
        ints[0].Loops[0].Length.AssertEqualsTol(tol, 21.16635090);

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
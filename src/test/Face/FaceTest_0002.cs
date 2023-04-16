namespace SearchAThing.Sci.Tests;

public partial class LoopTests
{

    [Fact]
    public void FaceTest_0002()
    {
        DxfDocument? outdxf = null;
        // outdxf = new DxfDocument();

        var dxf = netDxf.DxfDocument.Load(
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Face/FaceTest_0002.dxf"));

        var tol = 1e-8;

        var faces = dxf.Entities.Polylines2D.Select(lwp => new Face(lwp.ToLoop(tol))).ToList();

        var faceYellow = new Face(dxf.Entities.Polylines2D.First(w => w.Color.Index == AciColor.Yellow.Index).ToLoop(tol));
        var faceGreen = new Face(dxf.Entities.Polylines2D.First(w => w.Color.Index == AciColor.Green.Index).ToLoop(tol));

        var midplane = faceYellow.Plane.MiddlePlane(tol, faceGreen.Plane);
        outdxf?.AddEntities(midplane.CS.ToDxfLines(10));

        outdxf?.AddEntities(faceYellow.DxfEntities(tol).ToList().Act(ent => { foreach (var x in ent) x.Color = AciColor.Yellow; }));
        outdxf?.AddEntities(faceGreen.DxfEntities(tol).ToList().Act(ent => { foreach (var x in ent) x.Color = AciColor.Green; }));

        var faceYellowProjected = faceYellow.Project(tol, midplane);
        outdxf?.AddEntities(faceYellowProjected.DxfEntities(tol).ToList().Act(ent => { foreach (var x in ent) x.Color = AciColor.Yellow; }));

        var faceGreenProjected = faceGreen.Project(tol, midplane);
        outdxf?.AddEntities(faceGreenProjected.DxfEntities(tol).ToList().Act(ent => { foreach (var x in ent) x.Color = AciColor.Green; }));

        var prjInt = faceYellowProjected.Boolean(tol, faceGreenProjected, Face.BooleanMode.Intersect).ToList();
        Assert.True(prjInt.Count == 1 && prjInt[0].Loops.Count == 1);
        prjInt[0].Area.AssertEqualsTol(tol, faceYellowProjected.Area);

        if (outdxf != null)
        {
            outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
            outdxf.Viewport.ShowGrid = false;
            outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
        }

        ;
    }

}
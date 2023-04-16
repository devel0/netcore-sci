namespace SearchAThing.Sci.Tests;

public partial class LoopTests
{

    [Fact]
    public void LoopTest_0004()
    {
        DxfDocument? outdxf = null;
        //outdxf = new DxfDocument();

        var tol = 1e-1;

        var faceGreen = new Loop(tol, new[]
        {
                new Line3D(1,1,0, 1,1,10),
                new Line3D(1,1,10, 6,1,10),
                new Line3D(6,1,10, 6,1,0),
                new Line3D(6,1,0, 1,1,0)
            }).ToFace();

        var faceYellow = new Loop(tol, new[]
        {
                new Line3D(1,1,0, 1,1,10),
                new Line3D(1,1,10, 4,1,10),
                new Line3D(4,1,10, 4,1,0),
                new Line3D(4,1,0, 1,1,0)
            }).ToFace();

        var gyInts = faceGreen.Boolean(tol, faceYellow).ToList();

        Assert.True(gyInts.Count == 1);

        if (outdxf != null)
        {
            outdxf.AddEntity(faceGreen.Loops[0].DxfEntity(tol).Act(w => w.SetColor(AciColor.Green)));
            outdxf.AddEntity(faceYellow.Loops[0].DxfEntity(tol).Act(w => w.SetColor(AciColor.Yellow)));

            foreach (var gyInt in gyInts)
            {
                outdxf.AddEntity(gyInt
                    .Loops[0]
                    .ToHatch(tol, new HatchPattern(HatchPattern.Line.Name) { Angle = 45, Scale = 0.5 })
                    .SetColor(AciColor.Cyan));
            }
        }

        gyInts[0].Loops[0].Area.AssertEqualsTol(tol, 30);

        if (outdxf != null)
        {
            outdxf.DrawingVariables.PdMode = netDxf.Header.PointShape.CircleCross;
            outdxf.Viewport.ShowGrid = false;
            outdxf.Save(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "out.dxf"));
        }

    }

}
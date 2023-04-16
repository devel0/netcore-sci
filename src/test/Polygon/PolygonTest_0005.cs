namespace SearchAThing.Sci.Tests;

public partial class PolygonTests
{

    [Fact]
    public void PolygonTest_0005()
    {
        var dxf = netDxf.DxfDocument.Load(
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Polygon/PolygonTest_0005.dxf"));

        var tol = 1e-8;

        var poly = dxf.Entities.Polylines2D.First().Vertexes.Select(w => new Vector3D(w.Position)).ToList();
        if (poly.Last().EqualsTol(tol, poly.First())) poly.RemoveAt(poly.Count - 1);

        var loop = dxf.Entities.Polylines2D.First().ToLoop(tol);

        var outsidePts = new List<Vector3D>();

        foreach (var dxfpt in dxf.Entities.Points)
        {
            if (dxfpt.Color.Index == AciColor.Magenta.Index) outsidePts.Add(new Vector3D(dxfpt.Position));
        }

        foreach (var point in outsidePts)
        {
            var qpoly = poly.ContainsPoint(tol, point);
            var qloop = loop.ContainsPoint(tol, point, LoopContainsPointMode.InsideExcludedPerimeter);

            Assert.False(qpoly);
            Assert.False(qloop);
        }

    }

}
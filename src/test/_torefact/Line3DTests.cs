namespace SearchAThing.Sci.Tests;

public partial class Line3DTests_torefact
{

    double rad_tol;

    public Line3DTests_torefact()
    {
        rad_tol = (1e-1).ToRad();
    }

    /// <summary>
    /// 3d line by given two vectors, first is from, second is to
    /// </summary>
    [Fact]
    public void Line3DTest_001()
    {
        var l = new Line3D(new Vector3D(1, 2, 3), new Vector3D(4, 5, 6));
        Assert.True(l.From.EqualsTol(1e-1, 1, 2, 3) && l.To.EqualsTol(1e-1, 4, 5, 6));
    }

    /// <summary>
    /// 3d line by 1 point and 1 vector, first is start, second summed up to result in to
    /// </summary>
    [Fact]
    public void Line3DTest_002()
    {
        var l = new Line3D(new Vector3D(1, 2, 3), new Vector3D(4, 5, 6), Line3DConstructMode.PointAndVector);
        Assert.True(l.From.EqualsTol(1e-1, 1, 2, 3) && l.To.EqualsTol(1e-1, 1 + 4, 2 + 5, 3 + 6));
    }

    /// <summary>
    /// 3d line by two 2d tuples ( z zero )
    /// </summary>
    [Fact]
    public void Line3DTest_003()
    {
        var l = new Line3D(1, 2, 3, 4);
        Assert.True(l.From.EqualsTol(1e-1, 1, 2, 0) && l.To.EqualsTol(1e-1, 3, 4, 0));
    }

    /// <summary>
    /// 3d line by two 3d tuples ( from is x1,y1,z1 while to is x2,y2,z2 )
    /// </summary>
    [Fact]
    public void Line3DTest_004()
    {
        var l = new Line3D(1, 2, 3, 4, 5, 6);
        Assert.True(l.From.EqualsTol(1e-1, 1, 2, 3) && l.To.EqualsTol(1e-1, 4, 5, 6));
    }

    /// <summary>
    /// 3d line equality
    /// </summary>
    [Fact]
    public void Line3DTest_005()
    {
        var l = new Line3D(1, 2, 3, 4, 5, 6);
        Assert.True(l.EqualsTol(1e-1, new Line3D(new Vector3D(1, 2, 3), new Vector3D(4, 5, 6))));
    }

    /// <summary>
    /// consecutive segment states which is the common point
    /// or null if there isn't in the used tolerance
    /// </summary>
    [Fact]
    public void Line3DTest_006()
    {
        var l = new Line3D(1, 2, 3, 4, 5, 6);
        var l2 = new Line3D(4, 5, 6, 7, 8, 9);
        Assert.True(l.CommonPoint(1e-1, l2).Act(w => Assert.NotNull(w))!.EqualsTol(1e-1, 4, 5, 6));
        var l3 = new Line3D(4.11, 5.11, 6.11, 7.11, 8.11, 9.11);
        // common point test only from,to
        Assert.True(l.CommonPoint(1e-1, l3) is null);
    }

    /// <summary>
    /// revert a 3d line swaps from,to
    /// </summary>
    [Fact]
    public void Line3DTest_007()
    {
        var l = new Line3D(1, 2, 3, 4, 5, 6);
        var r = l.Reverse();
        Assert.True(r.From.EqualsTol(1e-1, 4, 5, 6) && r.To.EqualsTol(1e-1, 1, 2, 3));
    }

    /// <summary>
    /// inverted line retrieve a line with same from but to inverted respect to from
    /// Line3DTest_008.dxf
    /// </summary>
    [Fact]
    public void Line3DTest_008()
    {
        var l = new Line3D(1, 2, 3, 4, 5, 6);
        var i = l.Inverted;
        Assert.True(i.From.EqualsTol(1e-1, 1, 2, 3) && i.To.EqualsTol(1e-1, -2, -1, 0));
    }

    /// <summary>
    /// scale line respect a given point
    /// Line3DTest_009.dxf
    /// </summary>
    [Fact]
    public void Line3DTest_009()
    {
        var l = new Line3D(
            new Vector3D(16.423, 80.164, -18.989),
            new Vector3D(218.367, 151.378, 63.243));

        var p = new Vector3D(119.015, 54.432, 5.66);
        var l2 = l.Scale(p, .71);
        Assert.True(l2.EqualsTol(1e-3, new Line3D(
            new Vector3D(46.175, 72.702, -11.841),
            new Vector3D(189.555, 123.264, 46.544))));

        var l4 = l.Scale(p, -1.5);
        Assert.True(l4.EqualsTol(1e-3, new Line3D(
            new Vector3D(272.903, 15.834, 42.634),
            new Vector3D(-30.013, -90.987, -80.715))));
    }

    [Fact]
    public void LineContainsPointTest()
    {
        var l = new Line3D(1.1885, -.6908, 1.0009, 3.0186, 7.0544, 4.4160);
        var p = new Vector3D(2.1035, 3.1818, 2.7085);
        Assert.True(l.LineContainsPoint(1e-4, p.X, p.Y, p.Z));
        Assert.True(l.LineContainsPoint(1e-4, p));

        // line contains point consider infinite line
        p = new Vector3D(.2734, -4.5634, -.7066);
        Assert.True(l.LineContainsPoint(1e-4, p.X, p.Y, p.Z));
        Assert.True(l.LineContainsPoint(1e-4, p));
    }

    [Fact]
    public void SegmentContainsPointTest()
    {
        var l = new Line3D(1.1885, -.6908, 1.0009, 3.0186, 7.0544, 4.4160);
        var p = new Vector3D(2.1035, 3.1818, 2.7085);
        Assert.True(l.SegmentContainsPoint(1e-4, p.X, p.Y, p.Z));
        Assert.True(l.SegmentContainsPoint(1e-4, p));

        // line contains point consider infinite line
        p = new Vector3D(.2734, -4.5634, -.7066);
        Assert.False(l.SegmentContainsPoint(1e-4, p.X, p.Y, p.Z));
        Assert.False(l.SegmentContainsPoint(1e-4, p));
    }

    [Fact]
    public void IntersectTest()
    {
        {
            var l = new Line3D(0, 0, 0, 10, 0, 0);
            var l2 = new Line3D(5, 1e-1, 0, 5, 1e-1, 10); // vertical line dst=1e-1

            // default intersection behavior : midpoint
            var ip = l.Intersect(1e-1, l2).Act(w => Assert.NotNull(w))!;
            Assert.True(ip.EqualsTol(1e-2, l.Intersect(1e-1, l2, LineIntersectBehavior.MidPoint)));
            Assert.True(ip.EqualsTol(1e-2, 5, 1e-1 / 2, 0));

            ip = l.Intersect(1e-1, l2, LineIntersectBehavior.PointOnThis).Act(w => Assert.NotNull(w))!;
            Assert.True(ip.EqualsTol(1e-2, 5, 0, 0));

            ip = l.Intersect(1e-1, l2, LineIntersectBehavior.PointOnOther).Act(w => Assert.NotNull(w))!;
            Assert.True(ip.EqualsTol(1e-2, 5, 1e-1, 0));

            Assert.True(l.Intersect(5e-2, l2) is null);
        }

        {
            var l = new Line3D(0, 0, 0, 10, 20, 30);

            // cs around l=cs.z
            var cs = new CoordinateSystem3D(l.From, l.V, CoordinateSystem3DAutoEnum.AAA);

            // build a per line offsetted
            var lperp_off = (Line3D)new Line3D(l.MidPoint, cs.BaseX, Line3DConstructMode.PointAndVector)
                .Move(cs.BaseY * 2e-1);

            var ip = l.Intersect(2e-1, lperp_off).Act(w => Assert.NotNull(w))!;
            Assert.True(ip.EqualsTol(1e-4, 4.9641, 9.9283, 15.0598));

            Assert.True(l.Intersect(1e-1, lperp_off) is null);
        }
    }

    [Fact]
    public void IntersectTest1()
    {
        var l1 = new Line3D(0, 0, 0, 10, 0, 0);
        var l2 = new Line3D(5, -5, 0, 5, -10, 0);

        Assert.True(l1.Intersect(1e-1, l2, thisSegment: true, otherSegment: true) is null);
        Assert.True(l1.Intersect(1e-1, l2, thisSegment: false, otherSegment: true) is null);
        Assert.True(l1.Intersect(1e-1, l2, thisSegment: true, otherSegment: false)
            .Act(w => Assert.NotNull(w))!
            .EqualsTol(1e-1, 5, 0, 0));
        Assert.True(l1.Intersect(1e-1, l2, thisSegment: false, otherSegment: false)
            .Act(w => Assert.NotNull(w))!
            .EqualsTol(1e-1, 5, 0, 0));
    }

    [Fact]
    public void IntersectTest2()
    {
        var l1 = new Line3D(0, 0, 0, 10, 0, 0);
        var l2 = new Line3D(5, -5, 0, 5, -10, 0);

        Assert.True(l1.Intersect(1e-1, l2, thisSegment: true, otherSegment: true) is null);
        Assert.True(l1.Intersect(1e-1, l2, thisSegment: false, otherSegment: true) is null);
        Assert.True(l1.Intersect(1e-1, l2, thisSegment: true, otherSegment: false)
            .Act(w => Assert.NotNull(w))!
            .EqualsTol(1e-1, 5, 0, 0));
        Assert.True(l1.Intersect(1e-1, l2, thisSegment: false, otherSegment: false)
            .Act(w => Assert.NotNull(w))!
            .EqualsTol(1e-1, 5, 0, 0));
    }

    [Fact]
    public void IntersectTest3()
    {
        var l1 = new Line3D(3.4319, 6.0508, -4.7570, -0.4533, 1.3145, -1.4730);
        var l2 = new Line3D(3.4319, 6.0508, -4.7570, 10.3895, -.5522, 3.7099);
        var pl = new Plane3D(new CoordinateSystem3D(l1.From, l1.V, l2.V));

        var l3 = new Line3D(1.3529, 5.5732, 0, 6.3351, .4489, -7.8931);

        var q = l3.Intersect(1e-1, pl).Act(w => Assert.NotNull(w))!;
        // intersect point of line w/plane is on the plane
        Assert.True(q.ToUCS(pl.CS).Z.EqualsTol(1e-4, 0));
        Assert.True(q.EqualsTol(1e-4, 3.0757, 3.8013, -2.7293));
    }

    [Fact]
    public void PerpendicularTest()
    {
        {
            var l = new Line3D(0, 0, 0, 10, 0, 0);
            var p = new Vector3D(5, 10, 0);
            var lperp = l.Perpendicular(1e-1, p).Act(w => Assert.NotNull(w))!;

            // perpendicular segment From equals to the given point
            Assert.True(lperp.From.EqualsTol(1e-1, p));

            // perpendicular segment end to the line which is perpendicular
            Assert.True(l.SegmentContainsPoint(1e-1, lperp.To));

            // two seg are perpendicular
            Assert.True(l.V.IsPerpendicular(lperp.V));
        }

        {
            var l = new Line3D(new Vector3D(441.37, 689.699, -179.739), new Vector3D(695.01, 759.599, 301.543));
            var p = new Vector3D(740.754, 286.803, 687.757);
            var lPerp = l.Perpendicular(1e-2, p).Act(w => Assert.NotNull(w))!;

            var ip = l.Intersect(1e-2, lPerp).Act(w => Assert.NotNull(w))!;
            Assert.True(l.LineContainsPoint(1e-2, ip!));
            Assert.True(l.V.IsPerpendicular(lPerp.V));
        }
    }

    [Fact]
    public void SemiLineContainsPointTest()
    {
        var l = new Line3D(0, 0, 0, 10, 10, 10);

        Assert.True(l.SemiLineContainsPoint(1e-1, new Vector3D(9, 9, 9)));
        Assert.False(l.SemiLineContainsPoint(1e-1, new Vector3D(-1, -1, -1)));
    }

    [Fact]
    public void ColinearTest()
    {
        var l = new Line3D(0, 0, 0, 10, 0, 0);
        var l2 = new Line3D(20, 0, 0, 30, 0, 0).Scale(new Vector3D(20, 0, 0), 1e6);

        Assert.True(l.Colinear(1e-1, l2));
        Assert.True(l2.Colinear(1e-1, l));

        var l3 = l.RotateAboutAxis(new Line3D(0, 0, 0, 0, 0, 1), 1e-1);
        Assert.False(l.Colinear(1e-1, l3));
        Assert.False(l2.Colinear(1e-1, l3));
    }

    [Fact]
    public void IsParallelToTest()
    {
        var l = new Line3D(0, 0, 0, 10, 0, 0);

        Assert.True(l.IsParallelTo(1e-4, Plane3D.XY));
        Assert.True(l.IsParallelTo(1e-4, Plane3D.XZ));
        Assert.False(l.IsParallelTo(1e-4, Plane3D.YZ));
    }

    [Fact]
    public void RotateAboutAxisTest()
    {
        var l = new Line3D(0, 0, 0, 10, 0, 0);
        var axis = new Line3D(3.6969, 2.5012, 0, 2.0024, 5.8572, 10);
        var lrot = l.RotateAboutAxis(axis, (45d).ToRad());
        Assert.True(lrot.EqualsTol(1e-4, new Line3D(2.7475, -1.7326, 1.0470, 9.8923, 4.7402, -1.6091)));
    }

    [Fact]
    public void SetLengthTest()
    {
        var l = new Line3D(0, 0, 0, 10, 0, 0);
        var l2 = l.SetLength(20);
        Assert.True(l2.From.EqualsTol(1e-1, 0, 0, 0));
        Assert.True(l2.To.EqualsTol(1e-1, 20, 0, 0));
    }

    [Fact]
    public void MoveTest()
    {
        var l = new Line3D(1, 2, 3, 10, 0, 0);
        var delta = new Vector3D(10, 20, 30);
        var l2 = (Line3D)(l.Move(delta));
        Assert.True(l2.From.EqualsTol(1e-1, l.From + delta));
        Assert.True(l2.V.EqualsTol(1e-1, l.V));
    }

    [Fact]
    public void MoveMidpointTest()
    {
        var l = new Line3D(0, 0, 0, 10, 0, 0);
        var l2 = l.MoveMidpoint(new Vector3D(0, 0, 0));
        Assert.True(l2.From.EqualsTol(1e-1, -5, 0, 0));
        Assert.True(l2.To.EqualsTol(1e-1, 5, 0, 0));
    }

    [Fact]
    public void SplitTest()
    {
        {
            var l = new Line3D(0, 0, 0, 10, 0, 0);
            var segs = l.Split(1e-1, new[] { new Vector3D(2, 0, 0), new Vector3D(8, 0, 0) }).Cast<Line3D>();
            Assert.True(segs.Count() == 3);
            Assert.True(segs.First().EqualsTol(1e-1, new Line3D(0, 0, 0, 2, 0, 0)));
            Assert.True(segs.Skip(1).First().EqualsTol(1e-1, new Line3D(2, 0, 0, 8, 0, 0)));
            Assert.True(segs.Last().EqualsTol(1e-1, new Line3D(8, 0, 0, 10, 0, 0)));
        }

        {
            var l = new Line3D(0, 0, 0, 10, 0, 0);
            var segs = l.Split(1e-1, new[]
            {
                    // external of extreme points skipped
                    new Vector3D(-1, 0, 0), new Vector3D(0, 0, 0), new Vector3D(10, 0, 0),

                    new Vector3D(2, 0, 0),
                    new Vector3D(8, 0, 0)
                }).Cast<Line3D>();
            Assert.True(segs.Count() == 3);
            Assert.True(segs.First().EqualsTol(1e-1, new Line3D(0, 0, 0, 2, 0, 0)));
            Assert.True(segs.Skip(1).First().EqualsTol(1e-1, new Line3D(2, 0, 0, 8, 0, 0)));
            Assert.True(segs.Last().EqualsTol(1e-1, new Line3D(8, 0, 0, 10, 0, 0)));
        }

        {
            var l = new Line3D(0, 0, 0, 10, 0, 0);
            var segs = l.Split(1e-1, new[] { new Vector3D(8, 0, 0), new Vector3D(2, 0, 0) }).Cast<Line3D>();
            Assert.True(segs.Count() == 3);
            // splitted segments start from begin of line
            Assert.True(segs.First().EqualsTol(1e-1, new Line3D(0, 0, 0, 2, 0, 0)));
            Assert.True(segs.Skip(1).First().EqualsTol(1e-1, new Line3D(2, 0, 0, 8, 0, 0)));
            Assert.True(segs.Last().EqualsTol(1e-1, new Line3D(8, 0, 0, 10, 0, 0)));
        }
    }

    [Fact]
    public void EnsureFromTest()
    {
        var l = new Line3D(0, 0, 0, 10, 0, 0);

        var l2 = l.EnsureFrom(1e-1, new Vector3D(0, 0, 0));
        Assert.True(l2.EqualsTol(1e-1, new Line3D(0, 0, 0, 10, 0, 0)));

        // return reversed segment
        var l3 = l.EnsureFrom(1e-1, new Vector3D(10, 0, 0));
        Assert.True(l3.EqualsTol(1e-1, new Line3D(10, 0, 0, 0, 0, 0)));

        try
        {
            // exception expected
            var l4 = l.EnsureFrom(1e-1, new Vector3D(-1, 0, 0));
            Assert.True(false);
        }
        catch
        {
            Assert.True(true);
        }
    }

    [Fact]
    public void ToStringTest()
    {
        var l = new Line3D(1, 1, 1, 1.12, 2.23, 3.391);
        // 3 digits default
        var s = l.ToString();
        Assert.True(s == "[Line3D] SFROM[(1, 1, 1)] STO[(1.12, 2.23, 3.391)] L=2.692 Δ=(0.12, 1.23, 2.391)");

        // 1 digits explicit
        var s2 = l.ToString(1);
        Assert.Equal("[Line3D] SFROM[(1, 1, 1)] STO[(1.1, 2.2, 3.4)] L=2.7 Δ=(0.1, 1.2, 2.4)", s2);

        l = l.ToggleSense();

        var s_s2 = l.ToString(1);
        Assert.Equal("[Line3D] !S SFROM[(1.1, 2.2, 3.4)] STO[(1, 1, 1)] L=2.7 Δ=(-0.1, -1.2, -2.4)", s_s2);
    }

    [Fact]
    public void ToStringTest1()
    {
        var l = new Line3D(1, 1, 1, 1.12, 2.23, 3.391);
        // 1e-1 tolerance
        var s = l.ToStringTol(1e-1);
        Assert.True(s == "(1, 1, 1)_(1.1, 2.2, 3.4)");
    }

    [Fact]
    public void NormalizedTest()
    {
        var l = new Line3D(10, 20, 30, 40, 50, 60);
        var ln = l.Normalized();
        Assert.True(ln.From.EqualsTol(1e-6, 10, 20, 30));
        Assert.True(ln.V.EqualsTol(1e-6, new Vector3D(1, 1, 1).Normalized()));
    }

    [Fact]
    public void DivideTest()
    {
        var l = new Line3D(10, 20, 30, 40, 50, 60);
        // divide 3 equal parts (default: w/out endpoints) results in 2 pts
        {
            var pts = l.Divide(3);
            Assert.True(pts.Count() == 2);
            Assert.True(pts.First().EqualsTol(1e-6, (l.Normalized() * (l.Length / 3)).To));
            Assert.True(pts.Skip(1).First().EqualsTol(1e-6, (l.Normalized() * (l.Length / 3 * 2)).To));
        }

        // divide 3 equal parts include endpoints results in 4 pts
        {
            var pts = l.Divide(3, include_endpoints: true);
            Assert.True(pts.Count() == 2 + 2);
            Assert.True(pts.First().EqualsTol(1e-6, l.From));
            Assert.True(pts.Skip(1).First().EqualsTol(1e-6, (l.Normalized() * (l.Length / 3)).To));
            Assert.True(pts.Skip(2).First().EqualsTol(1e-6, (l.Normalized() * (l.Length / 3 * 2)).To));
            Assert.True(pts.Last().EqualsTol(1e-6, l.To));
        }
    }

    [Fact]
    public void BBoxTest()
    {
        var l = new Line3D(10, 50, -10, -10, -50, 60);
        var bbox = l.BBox(1e-1);
        Assert.True(bbox.Min.EqualsTol(1e-1, -10, -50, -10));
        Assert.True(bbox.Max.EqualsTol(1e-1, 10, 50, 60));
    }

    [Fact]
    public void BisectTest()
    {
        {
            var l1 = new Line3D(-.2653, 6.7488, 1.4359, 1.8986, 3.1188, 1.0844);
            var l2 = new Line3D(1.8986, 3.1188, 1.0844, 8.1864, 2.4120, 1.7818);
            var bisect = l1.Bisect(1e-4, l2).Act(w => Assert.NotNull(w))!;

            // bisect segment from start from the intersection point of two lines
            var ip = l1.CommonPoint(1e-4, l2).Act(w => Assert.NotNull(w))!;

            Assert.True(bisect.From.EqualsTol(1e-4, ip));

            var l1_from_ip = l1.EnsureFrom(1e-4, ip);
            var l2_from_ip = l2.EnsureFrom(1e-4, ip);

            // angle from bisect toward l1 equals angle from bisect toward l2
            var bisect_ang_l1 = bisect.V.AngleRad(1e-4, l1_from_ip.V);
            var bisect_ang_l2 = bisect.V.AngleRad(1e-4, l2_from_ip.V);
            Assert.True(bisect_ang_l1.EqualsTol(rad_tol, bisect_ang_l2));
        }

        {
            var l1 = new Line3D(0, 0, 0, 10, 0, 0);
            var l2 = new Line3D(0, 0, 0, -5, 0, 0);
            {
                var bisect = l1.Bisect(1e-4, l2);
                // two parallel lines not form a plane, then a fallback rotation axis must be given
                Assert.True(bisect is null);
            }
            {
                // rotate right-hand Z+
                var bisect = l1.Bisect(1e-4, l2, Vector3D.ZAxis).Act(w => Assert.NotNull(w))!;
                Assert.True(bisect.EqualsTol(1e-4, new Line3D(0, 0, 0, 0, 10, 0)));
            }
            {
                // rotate right-hand Z-
                var bisect = l1.Bisect(1e-4, l2, -Vector3D.ZAxis).Act(w => Assert.NotNull(w))!;
                Assert.True(bisect.EqualsTol(1e-4, new Line3D(0, 0, 0, 0, -10, 0)));
            }
        }
    }

    [Fact]
    public void LineOffseted()
    {
        var l = new Line3D(new Vector3D(441.37, 689.699, -179.739), new Vector3D(695.01, 759.599, 301.543));
        var p = new Vector3D(740.754, 286.803, 687.757);

        var off = 15.5;
        var lOff = (Line3D)(l.Offset(1e-2, p, off));

        Assert.True(l.V.IsParallelTo(1e-2, lOff.V));

        var dLO = l.From.Distance(1e-2, lOff);
        var dOL = lOff.From.Distance(1e-2, l);
        Assert.True(dLO.EqualsTol(1e-2, dOL));
        Assert.True(dOL.EqualsTol(1e-2, off));
    }

}
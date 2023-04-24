namespace SearchAThing.Sci;

/// <summary>
/// helper class to hold triangle info, used in conjunction with [LibTessDotNet](https://github.com/speps/LibTessDotNet) methods.
/// </summary>
public class Triangle3D
{

    public Vector3D V1 { get; private set; }

    public Vector3D V2 { get; private set; }

    public Vector3D V3 { get; private set; }

    CoordinateSystem3D? _CS = null;
    /// <summary>
    /// (cached) on demand coordinate system with origin V1.
    /// </summary>    
    public CoordinateSystem3D CS
    {
        get
        {
            if (_CS is null)
                _CS = new CoordinateSystem3D(V1, V2 - V1, V3 - V1, SmartCsMode.X_YQ);

            return _CS;
        }
    }

    public Triangle3D(Vector3D v1, Vector3D v2, Vector3D v3)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
    }

    /// <summary>
    /// states if this triangle parallel to other given.
    /// </summary>    
    public bool IsParallelTo(double tol, Triangle3D other) =>
        CS.BaseZ.IsParallelTo(tol, other.CS.BaseZ);

    /// <summary>
    /// Retrieve (v1,v2), (v2,v3), (v3,v1) triangle segments.
    /// </summary>    
    public IEnumerable<Line3D> Segments
    {
        get
        {
            yield return new Line3D(V1, V2);
            yield return new Line3D(V2, V3);
            yield return new Line3D(V3, V1);
        }
    }

    /// <summary>
    /// States if a point is in this triangle.
    /// </summary>    
    public bool Contains(Vector3D point)
    {
        var a = V1 - point;
        var b = V2 - point;
        var c = V3 - point;

        var u = b.CrossProduct(c);
        var v = c.CrossProduct(a);
        var w = a.CrossProduct(b);

        if (u.DotProduct(v) < 0 || u.DotProduct(w) < 0)
            return false;

        return true;
    }

    /// <summary>
    /// intersect this triangle with given other, returning intersection line 
    /// ( eventyally a point line ) or null if no intersects at all.
    /// </summary>    
    public Line3D? Intersect(double tol, Triangle3D other)
    {
        if (this.IsParallelTo(tol, other)) return null;

        Line3D? res = null;
        {
            var ips = new List<Vector3D>();

            foreach (var segment in Segments)
            {
                var ip = segment.Intersect(tol, other.CS);
                if (ip is not null && segment.SegmentContainsPoint(tol, ip))
                    ips.Add(ip);
            }

            if (ips.Count == 1) return new Line3D(ips[0], ips[0]);

            if (ips.Count == 2) res = new Line3D(ips[0], ips[1]);
            if (ips.Count > 2)
            {
                var ip_a = ips[0];
                var ip_b = ips.Skip(1).OrderByDescending(ip => (ip - ip_a).Length).First();
                res = new Line3D(ip_a, ip_b);
            }
        }

        if (res is not null) return other.RestrictTo(tol, res);

        return res;
    }

    /// <summary>
    /// Restrict given this triangle coplanar line to be enclosed in the triangle shape.
    /// </summary>
    /// <param name="tol">Length comparision tolerance.</param>
    /// <param name="line">This triangle coplanar line.</param>
    /// <returns>Segment of line enclosed in this triangle.</returns>
    public Line3D? RestrictTo(double tol, Line3D line)
    {
        var ips = Segments
            .Select(segment => new { segment, ip = segment.Intersect(tol, line) })
            .Where(nfo => 
                nfo.ip is not null && 
                nfo.segment.SegmentContainsPoint(tol, nfo.ip) &&
                line.SegmentContainsPoint(tol, nfo.ip))
            .Select(nfo => nfo.ip!)
            .ToList();

        if (this.Contains(line.From)) ips.Add(line.From);
        if (this.Contains(line.To)) ips.Add(line.To);

        var vcmp = new Vector3DEqualityComparer(tol);
        ips = ips.Distinct(vcmp).ToList();

        if (ips.Count == 2)
            return new Line3D(ips[0], ips[1]);

        return null;
    }

}
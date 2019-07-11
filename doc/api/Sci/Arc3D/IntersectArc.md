# SearchAThing.Sci.Arc3D.IntersectArc method
## IntersectArc(double, CoordinateSystem3D)
find ips of intersect this arc to the given cs plane; return empty set if arc cs plane parallel to other given cs

### Signature
```csharp
public System.Collections.Generic.IEnumerable<SearchAThing.Sci.Vector3D> IntersectArc(double tol, CoordinateSystem3D cs)
```
### Parameters
- `tol`: len tolerance
- `cs`: cs xy plane

## IntersectArc(double, Line3D, bool, bool)
states if this arc intersect given line

### Signature
```csharp
public System.Collections.Generic.IEnumerable<SearchAThing.Sci.Vector3D> IntersectArc(double tol, Line3D l, bool segment_mode = False, bool arc_mode = True)
```
### Parameters
- `tol`: arc tolerance
- `l`: line to test intersect
- `segment_mode`: if true line treat as segment instead of infinite
- `arc_mode`: if true arc goes from-to ; if false arc treat as circle


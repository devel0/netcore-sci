# SearchAThing.Sci.Circle3D.Intersect method
## Intersect(double, CoordinateSystem3D, bool)
find ips of intersect this arc to the given cs plane; 
            return empty set if arc cs plane parallel to other given cs

### Signature
```csharp
public override System.Collections.Generic.IEnumerable<SearchAThing.Sci.Vector3D> Intersect(double tol, CoordinateSystem3D cs, bool only_perimeter = True)
```
### Parameters
- `tol`: len tolerance
- `cs`: cs xy plane
- `only_perimeter`: if false it will check in the arc area too, otherwise only on arc perimeter

### Returns
sample
### Remarks
[unit test](/test/Arc3D/Arc3DTest_0001.cs)
            ![](/test/Arc3D/Arc3DTest_0001.png)
## Intersect(double, Line3D, bool, bool)
intersect this 3d circle with given 3d line

### Signature
```csharp
public virtual System.Collections.Generic.IEnumerable<SearchAThing.Sci.Vector3D> Intersect(double tol, Line3D l, bool only_perimeter = True, bool segment_mode = False)
```
### Returns

### Remarks


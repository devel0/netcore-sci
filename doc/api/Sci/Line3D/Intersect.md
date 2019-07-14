# SearchAThing.Sci.Line3D.Intersect method
## Intersect(double, CoordinateSystem3D)
returns null if this line is parallel to the cs xy plane,
            the intersection point otherwise

### Signature
```csharp
public SearchAThing.Sci.Vector3D Intersect(double tol, CoordinateSystem3D cs)
```
### Returns

### Remarks


<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## Intersect(double, Plane3D)
returns null if this line is parallel to the plane,
            the intersection point otherwise

### Signature
```csharp
public SearchAThing.Sci.Vector3D Intersect(double tol, Plane3D plane)
```
### Returns

### Remarks


<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## Intersect(double, Line3D, LineIntersectBehavior)
Find intersection point between this and other line using given tolerance.
            Returns null if no intersection, otherwise it returns a point on
            the shortest segment ( the one that's perpendicular to either lines )
            based on given behavior ( default midpoint ).

### Signature
```csharp
public SearchAThing.Sci.Vector3D Intersect(double tol, Line3D other, LineIntersectBehavior behavior = 0)
```
### Returns

### Remarks


<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## Intersect(double, Line3D, bool, bool)
Intersects two lines with arbitrary segment mode for each.

### Signature
```csharp
public SearchAThing.Sci.Vector3D Intersect(double tol, Line3D other, bool thisSegment, bool otherSegment)
```
### Returns

### Remarks


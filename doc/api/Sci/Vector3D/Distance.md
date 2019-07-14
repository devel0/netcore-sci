# SearchAThing.Sci.Vector3D.Distance method
## Distance(Vector3D)
compute distance between this point and the other given

### Signature
```csharp
public double Distance(Vector3D other)
```
### Returns

### Remarks
[unit test](/test/Vector3D/Vector3DTest_0014.cs)
## Distance(double, Line3D)
compute perpendicular(min) distance of this point from given line

### Signature
```csharp
public double Distance(double tol, Line3D other)
```
### Parameters
- `tol`: length tolerance ( used to check if point contained in line )
- `other`: line

### Returns

### Remarks
[unit test](/test/Vector3D/Vector3DTest_0015.cs)
            ![](/test/Vector3D/Vector3DTest_0015.png)

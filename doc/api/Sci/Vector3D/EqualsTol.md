# SearchAThing.Sci.Vector3D.EqualsTol method
## EqualsTol(double, Vector3D)
checks vector component equality vs other given           
            [unit test](/test/Vector3D/Vector3DTest_0012.cs)

### Signature
```csharp
public bool EqualsTol(double tol, Vector3D other)
```
### Parameters
- `tol`: geometric tolerance ( note: use Constants.NormalizedLengthTolerance )
- `other`: vector to compare to this

## EqualsTol(double, double, double)
checks only x,y
            [unit test](/test/Vector3D/Vector3DTest_0012.cs)

### Signature
```csharp
public bool EqualsTol(double tol, double x, double y)
```
## EqualsTol(double, double, double, double)
checks vector component equality vs other given            
            [unit test](/test/Vector3D/Vector3DTest_0012.cs)

### Signature
```csharp
public bool EqualsTol(double tol, double x, double y, double z)
```
### Parameters
- `tol`: geometric tolerance ( note: use Constants.NormalizedLengthTolerance )
- `x`: (No Description)
- `y`: (No Description)
- `z`: (No Description)


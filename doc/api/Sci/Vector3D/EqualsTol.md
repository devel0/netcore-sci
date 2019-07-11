# SearchAThing.Sci.Vector3D.EqualsTol method
## EqualsTol(double, Vector3D)
checks vector component equality vs other given

### Signature
```csharp
public bool EqualsTol(double tol, Vector3D other)
```
### Parameters
- `tol`: geometric tolerance ( note: use Constant.NormalizedLengthTolerance )
- `other`: vector to compare to this

## EqualsTol(double, double, double)
checks only x,y

### Signature
```csharp
public bool EqualsTol(double tol, double x, double y)
```
## EqualsTol(double, double, double, double)
checks vector component equality vs other given

### Signature
```csharp
public bool EqualsTol(double tol, double x, double y, double z)
```
### Parameters
- `tol`: geometric tolerance ( note: use Constant.NormalizedLengthTolerance )
- `x`: other vector ( x component )
- `y`: other vector ( y component )
- `z`: other vector ( z component )


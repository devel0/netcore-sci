# SearchAThing.Sci.CoordinateSystem3D.Equals method
## Equals(object)
### Signature
```csharp
public override bool Equals(object obj)
```
## Equals(double, CoordinateSystem3D)
verify is this cs is equals to otherByLayer ( same origin, x, y, z base vectors )
            [unit test](/test/Vector3D/Vector3DTest_0001.cs)

### Signature
```csharp
public bool Equals(double tol, CoordinateSystem3D other)
```
### Parameters
- `tol`: calc tolerance ( for origin check )
- `other`: cs to check equality against

### Returns
true if this cs equals the given on, false otherwise
### Remarks


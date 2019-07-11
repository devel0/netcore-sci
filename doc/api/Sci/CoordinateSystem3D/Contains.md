# SearchAThing.Sci.CoordinateSystem3D.Contains method
## Contains(double, Vector3D, bool)
verify if this cs XY plane contains given wcs point

### Signature
```csharp
public bool Contains(double tol, Vector3D point, bool evalCSOrigin = True)
```
### Parameters
- `tol`: calc tolerance
- `point`: point to verify
- `evalCSOrigin`: if true CS origin will subtracted before transform test


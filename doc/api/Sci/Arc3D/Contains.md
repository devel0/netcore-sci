# SearchAThing.Sci.Arc3D.Contains method
## Contains(double, Vector3D, bool)
states if given point relies on this arc perimeter or shape depending on arguments

### Signature
```csharp
public virtual bool Contains(double tol, Vector3D pt, bool onlyPerimeter)
```
### Parameters
- `tol`: length tolerance
- `pt`: point to check
- `onlyPerimeter`: if true it checks if point is on perimeter; if false it will check in area too


# SearchAThing.SciExt.Project method
## Project(Vector3D, CoordinateSystem3D, bool)
wcs coord of projected coord to the given cs

### Signature
```csharp
public static SearchAThing.Sci.Vector3D Project(Vector3D v, CoordinateSystem3D cs, bool evalCSOrigin = True)
```
### Parameters
- `v`: wcs point
- `cs`: cs to project
- `evalCSOrigin`: if true cs origin will subtracted before transform, then readded to obtain wcs point


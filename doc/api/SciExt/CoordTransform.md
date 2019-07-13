# SearchAThing.SciExt.CoordTransform method
## CoordTransform(DxfDocument, Func<SearchAThing.Sci.Vector3D, SearchAThing.Sci.Vector3D>)
### Signature
```csharp
public static System.Collections.Generic.IEnumerable<netDxf.Entities.EntityObject> CoordTransform(DxfDocument dxf, Func<SearchAThing.Sci.Vector3D, SearchAThing.Sci.Vector3D> transform)
```
## CoordTransform(EntityObject, Func<SearchAThing.Sci.Vector3D, SearchAThing.Sci.Vector3D>, Vector3D)
build a clone of the given entity with coord transformed accordingly given function.

### Signature
```csharp
public static netDxf.Entities.EntityObject CoordTransform(EntityObject eo, Func<SearchAThing.Sci.Vector3D, SearchAThing.Sci.Vector3D> transform, Vector3D origin = null)
```
### Returns

### Remarks


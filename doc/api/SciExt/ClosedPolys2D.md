# SearchAThing.SciExt.ClosedPolys2D method
## ClosedPolys2D(IEnumerable<SearchAThing.Sci.Line3D>, double, int)
build polygons from given list of segments
            if want to represent arcs, add them as dummy lines to segs
            polys returned are ordered anticlockwise

### Signature
```csharp
public static System.Collections.Generic.IEnumerable<System.Collections.Generic.IReadOnlyList<SearchAThing.Sci.Vector3D>> ClosedPolys2D(IEnumerable<SearchAThing.Sci.Line3D> segs, double tolLen, int polyMaxPoints = 0)
```

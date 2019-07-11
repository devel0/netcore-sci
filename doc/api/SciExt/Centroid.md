# SearchAThing.SciExt.Centroid method
## Centroid(IReadOnlyList<SearchAThing.Sci.Vector3D>, double)
Centroid of a polygon (does not consider z)
            note: points must ordered anticlockwise
            ( if have area specify the parameter to avoid recomputation )
            https://en.wikipedia.org/wiki/Centroid

### Signature
```csharp
public static SearchAThing.Sci.Vector3D Centroid(IReadOnlyList<SearchAThing.Sci.Vector3D> pts, double tol)
```
## Centroid(IReadOnlyList<SearchAThing.Sci.Vector3D>, double, double)
Centroid of a polygon (does not consider z)
            note: points must ordered anticlockwise
            https://en.wikipedia.org/wiki/Centroid

### Signature
```csharp
public static SearchAThing.Sci.Vector3D Centroid(IReadOnlyList<SearchAThing.Sci.Vector3D> pts, double tol, double area)
```

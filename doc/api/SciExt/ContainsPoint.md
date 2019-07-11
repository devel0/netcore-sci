# SearchAThing.SciExt.ContainsPoint method
## ContainsPoint(IReadOnlyList<SearchAThing.Sci.Vector3D>, double, Vector3D, bool)
states if the given polygon contains the test point ( z not considered )
            https://en.wikipedia.org/wiki/Point_in_polygon
            By default check the point contained in the polygon perimeter.
            Optionally duplicate points are zapped in comparing.

### Signature
```csharp
public static bool ContainsPoint(IReadOnlyList<SearchAThing.Sci.Vector3D> _pts, double tol, Vector3D _pt, bool zapDuplicates = False)
```

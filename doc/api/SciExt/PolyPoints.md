# SearchAThing.SciExt.PolyPoints method
## PolyPoints(IEnumerable<SearchAThing.Sci.Line3D>)
retrieve s[0].from, s[1].from, ... s[n-1].from, s[n-1].to points

### Signature
```csharp
public static System.Collections.Generic.IEnumerable<SearchAThing.Sci.Vector3D> PolyPoints(IEnumerable<SearchAThing.Sci.Line3D> segs)
```
## PolyPoints(IEnumerable<SearchAThing.Sci.Vector3D>, double, bool)
given a set of polygon pts, returns the enumeation of all pts
            so that the last not attach to the first ( if makeClosed = false ).
            Elsewhere it returns a last point equals the first ( makeClosed = true ).

### Signature
```csharp
public static System.Collections.Generic.IEnumerable<SearchAThing.Sci.Vector3D> PolyPoints(IEnumerable<SearchAThing.Sci.Vector3D> pts, double tol, bool makeClosed = False)
```

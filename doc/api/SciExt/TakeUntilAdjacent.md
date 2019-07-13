# SearchAThing.SciExt.TakeUntilAdjacent method
## TakeUntilAdjacent(IEnumerable<SearchAThing.Sci.Line3D>, double, bool)
Return the input set of segments until an adjacency between one and next is found.
            It can rectify the versus of line (by default) if needed.
            Note: returned set references can be different if rectifyVersus==true

### Signature
```csharp
public static System.Collections.Generic.IEnumerable<SearchAThing.Sci.Line3D> TakeUntilAdjacent(IEnumerable<SearchAThing.Sci.Line3D> segs, double tol, bool rectifyVersus = True)
```
### Returns

### Remarks


# SearchAThing.SciExt.MergeColinearSegments method
## MergeColinearSegments(IEnumerable<SearchAThing.Sci.Line3D>, double)
merge colinear overlapped segments into single
            result segments direction and order is not ensured
            pre: segs must colinear

### Signature
```csharp
public static System.Collections.Generic.IEnumerable<SearchAThing.Sci.Line3D> MergeColinearSegments(IEnumerable<SearchAThing.Sci.Line3D> _segs, double tol_len)
```

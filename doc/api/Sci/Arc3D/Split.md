# SearchAThing.Sci.Arc3D.Split method
## Split(double, IEnumerable<SearchAThing.Sci.Vector3D>, bool)
create a set of subarc from this by splitting through given split points
            split point are not required to be on perimeter of the arc ( a center arc to point line will split )
            generated subarcs will start from this arc angleFrom and contiguosly end to angleTo

### Signature
```csharp
public System.Collections.Generic.IEnumerable<SearchAThing.Sci.Arc3D> Split(double tol_len, IEnumerable<SearchAThing.Sci.Vector3D> _splitPts, bool validate_pts = False)
```
### Parameters
- `tol_len`: arc length tolerance
- `_splitPts`: point where split arc
- `validate_pts`: if true split only for split points on arc perimeter

### Returns

### Remarks


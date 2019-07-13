# SearchAThing.Sci.Line3D.ApparentIntersect method
## ApparentIntersect(Line3D)
Find apparent intersection between this and given other line
            returning (shortest) segment perpendicular to either lines or null if lines parallels.
            This method will used from Intersect to find intersection between lines when
            perpendicular segment length not exceed given length tolerance.
            [unit test](/test/Line3D/Line3DTest_0001.cs)
            ![](/test/Line3D/Line3DTest_0001.png)

### Signature
```csharp
public SearchAThing.Sci.Line3D ApparentIntersect(Line3D other)
```
### Parameters
- `other`: other 3d line

### Returns

### Remarks


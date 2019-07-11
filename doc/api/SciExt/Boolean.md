# SearchAThing.SciExt.Boolean method
## Boolean(IEnumerable<SearchAThing.Sci.Vector3D>, double, IEnumerable<SearchAThing.Sci.Vector3D>, ClipType, bool)
can generate a Int64MapExceptionRange exception if double values can't fit into a In64 representation.
            In that case try with tolerances not too small.
            It is suggested to use a lenTol/10 to avoid lost of precision during domain conversions.

### Signature
```csharp
public static System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<SearchAThing.Sci.Vector3D>> Boolean(IEnumerable<SearchAThing.Sci.Vector3D> polyA, double tol, IEnumerable<SearchAThing.Sci.Vector3D> polyB, ClipType type, bool selfCheckInt64MapTolerance = True)
```

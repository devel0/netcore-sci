# SearchAThing.SciExt.ZapDuplicates method
## ZapDuplicates(IEnumerable<SearchAThing.Sci.Vector3D>, double)
return pts (maintaining order) w/out duplicates
            use the other overloaded method if already have a vector 3d equality comparer

### Signature
```csharp
public static System.Collections.Generic.IEnumerable<SearchAThing.Sci.Vector3D> ZapDuplicates(IEnumerable<SearchAThing.Sci.Vector3D> pts, double tol)
```
## ZapDuplicates(IEnumerable<SearchAThing.Sci.Vector3D>, Vector3DEqualityComparer)
return pts (maintaining order) w/out duplicates

### Signature
```csharp
public static System.Collections.Generic.IEnumerable<SearchAThing.Sci.Vector3D> ZapDuplicates(IEnumerable<SearchAThing.Sci.Vector3D> pts, Vector3DEqualityComparer cmp)
```

# DisambiguatedPoints property (SearchAThing.Sci.Line3D)
retrieve a unique endpoint representation of this line3d segment (regardless its from-to or to-from order)
            such that From.Distance(Vector3D.Zero) less than To.Distance(Vector3D.Zero)

## Signature
```csharp
public IEnumerable<SearchAThing.Sci.Vector3D> DisambiguatedPoints
{
    get;
}
```

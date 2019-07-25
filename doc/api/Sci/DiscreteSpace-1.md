# DiscreteSpace<T> Class
**Namespace:** SearchAThing.Sci

**Inheritance:** Object → DiscreteSpace<T>

organize given item list into a discretized space to allow fast query of elements in a space region

## Signature
```csharp
public class DiscreteSpace
```
## Constructors
|**Name**|**Summary**|
|---|---|
|[DiscreteSpace<T>(double, IEnumerable<T>, Func<T, System.Collections.Generic.IEnumerable<SearchAThing.Sci.Vector3D>>, int)](DiscreteSpace-1/ctors.md)|Build a discrete space to search within GetItemsAt.<br/>            spaceDim need to equals 3 when using vector in 3d|
|[DiscreteSpace<T>(double, IEnumerable<T>, Func<T, SearchAThing.Sci.Vector3D>, int)](DiscreteSpace-1/ctors.md#discretespacetdouble-ienumerablet-funct-searchathingscivector3d-int)|Build a discrete space to search within GetItemsAt.<br/>            spaceDim need to equals 3 when using vector in 3d|
## Methods
|**Name**|**Summary**|
|---|---|
|[Equals](DiscreteSpace-1/Equals.md)||
|[GetHashCode](DiscreteSpace-1/GetHashCode.md)||
|[GetItemsAt](DiscreteSpace-1/GetItemsAt.md)|retrieve items that resides in the space at given point with given extents max distance|
|[GetType](DiscreteSpace-1/GetType.md)||
|[ToString](DiscreteSpace-1/ToString.md)||
## Conversions

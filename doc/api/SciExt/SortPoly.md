# SearchAThing.SciExt.SortPoly method
## SortPoly(IEnumerable<SearchAThing.Sci.Vector3D>, double, Vector3D)
### Signature
```csharp
public static System.Collections.Generic.IEnumerable<SearchAThing.Sci.Vector3D> SortPoly(IEnumerable<SearchAThing.Sci.Vector3D> pts, double tol, Vector3D refAxis = null)
```

<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## SortPoly(IEnumerable<SearchAThing.Sci.Line3D>, double, Vector3D)
Sort polygon segments so that they can form a polygon ( if they really form one ).
            It will not check for segment versus adjancency

### Signature
```csharp
public static System.Collections.Generic.IEnumerable<SearchAThing.Sci.Line3D> SortPoly(IEnumerable<SearchAThing.Sci.Line3D> segs, double tol, Vector3D refAxis = null)
```
### Returns

### Remarks


<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## SortPoly<T>(IEnumerable<T>, double, Func<T, SearchAThing.Sci.Vector3D>, Vector3D)
### Signature
```csharp
public static System.Collections.Generic.IEnumerable<T> SortPoly<T>(IEnumerable<T> pts, double tol, Func<T, SearchAThing.Sci.Vector3D> getPoint, Vector3D refAxis = null)
```

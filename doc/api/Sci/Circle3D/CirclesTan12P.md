# SearchAThing.Sci.Circle3D.CirclesTan12P method
## CirclesTan12P(double, Line3D, Line3D, Vector3D)
build 3d circle that tangent to lines t1,t2 and that intersects point p
            note: point p must contained in one of t1,t2
            circle will be inside region t1.V toward t2.V
            they are 4 circles

### Signature
```csharp
public static System.Collections.Generic.IEnumerable<SearchAThing.Sci.Circle3D> CirclesTan12P(double tol_len, Line3D t1, Line3D t2, Vector3D p)
```

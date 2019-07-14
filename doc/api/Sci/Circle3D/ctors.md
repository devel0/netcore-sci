# SearchAThing.Sci.Circle3D constructors
## Circle3D(double, CoordinateSystem3D, double)
### Signature
```csharp
public Circle3D(double tol_len, CoordinateSystem3D cs, double r)
```

<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## Circle3D(Arc3D)
create circle from given arc

### Signature
```csharp
public Circle3D(Arc3D arc)
```
### Parameters
- `arc`: arc used to build circle

### Remarks


<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## Circle3D(Vector3D, Vector3D, Vector3D)
Build 3d circle that intersect p1,p2,p3
            ( the inside CS will centered in the circle center and Xaxis toward p1 )

### Signature
```csharp
public Circle3D(Vector3D p1, Vector3D p2, Vector3D p3)
```
### Remarks


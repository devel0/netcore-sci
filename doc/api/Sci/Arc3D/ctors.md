# SearchAThing.Sci.Arc3D constructors
## Arc3D(double, CoordinateSystem3D, double, double, double)
construct 3d arc

### Signature
```csharp
public Arc3D(double tol_len, CoordinateSystem3D cs, double r, double angleRadStart, double angleRadEnd)
```
### Parameters
- `tol_len`: (No Description)
- `cs`: coordinate system with origin at arc center, XY plane of cs contains the arc, angle is 0 at cs x-axis and increase right-hand around cs z-axis
- `r`: arc radius
- `angleRadStart`: arc angle start (rad). is not required that start angle less than end. It will normalized 0-2pi
- `angleRadEnd`: arc angle end (rad). is not require that end angle great than start. It will normalized 0-2pi

### Remarks


<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## Arc3D(double, Vector3D, Vector3D, Vector3D, Vector3D)
Build arc by 3 given points
            ( the inside CS will centered in the arc center and Xaxis toward p1 )

### Signature
```csharp
public Arc3D(double tol_len, Vector3D p1, Vector3D p2, Vector3D p3, Vector3D normal = null)
```
### Remarks


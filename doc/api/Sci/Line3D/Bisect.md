# SearchAThing.Sci.Line3D.Bisect method
## Bisect(double, Line3D, Vector3D)
returns bisect of two given segment/lines
            ( if given segment not share nodes but intesects returned bisect start from ip )
            bisect choosen will be the one between this and other withing shortest angle
            
            if two given lines are parallel and parallelRotationAxis is given then
            bisect results as this segment rotated PI/2 about given axis using To as rotcenter

### Signature
```csharp
public SearchAThing.Sci.Line3D Bisect(double tol_len, Line3D other, Vector3D parallelRotationAxis = null)
```
### Returns

### Remarks


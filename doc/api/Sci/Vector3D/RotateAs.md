# SearchAThing.Sci.Vector3D.RotateAs method
## RotateAs(double, Vector3D, Vector3D, double, double)
Note: tol must be Constants.NormalizedLengthTolerance
            if comparing normalized vectors
            rotation from-to will be multiplied for given angleFactor ( default 1.0 )

### Signature
```csharp
public SearchAThing.Sci.Vector3D RotateAs(double tol, Vector3D from, Vector3D to, double angleFactor = 1d, double angleAddictionalRad = 0d)
```
### Parameters
- `tol`: geometric tolerance ( use Constants.NormalizedLengthTolerance if vectors are normalized )
- `from`: point from describing rotation path
- `to`: point to describing rotation path
- `angleFactor`: optional angle rotation scaler
- `angleAddictionalRad`: optional angle (rad) component (added after angleFactor scaler)

### Returns

### Remarks


# SearchAThing.Sci.Vector3D.AngleToward method
## AngleToward(double, Vector3D, Vector3D)
compute angle required to make this point go to the given one
            if rotate right-hand around given reference axis

### Signature
```csharp
public double AngleToward(double tolLen, Vector3D to, Vector3D refAxis)
```
### Parameters
- `tolLen`: geometric tolerance ( use Constants.NormalizedLengthTolerance if working with normalized vectors )
- `to`: point toward rotate this one
- `refAxis`: reference axis to make right-hand rotation of this point toward given one

### Returns
angle (rad)
### Remarks


# SearchAThing.Sci.Vector3D.AngleRad method
## AngleRad(double, Vector3D)
angle between this and given vector

### Signature
```csharp
public double AngleRad(double tolLen, Vector3D to)
```
### Parameters
- `tolLen`: geometric tolerance to test vector equalities ( use Constants.NormalizedLengthTolerance when comparing normalized vectors )
- `to`: other vector

### Returns
angle between two vectors (rad)
### Remarks
[unit test](/test/Vector3D/Vector3DTest_0020.cs)
            ![](/test/Vector3D/Vector3DTest_0020.png)

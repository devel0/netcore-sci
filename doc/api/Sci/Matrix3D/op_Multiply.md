# SearchAThing.Sci.Matrix3D.op_Multiply method
## operator *(double, Matrix3D)
scalar multiply

### Signature
```csharp
public static SearchAThing.Sci.Matrix3D operator *(double s, Matrix3D m)
```
## operator *(Matrix3D, double)
scalar multiply

### Signature
```csharp
public static SearchAThing.Sci.Matrix3D operator *(Matrix3D m, double s)
```
## operator *(Matrix3D, Vector3D)
matrix * vector as column -> vector
            3x3 x 3x1 -> 3x1

### Signature
```csharp
public static SearchAThing.Sci.Vector3D operator *(Matrix3D m, Vector3D v)
```
## operator *(Vector3D, Matrix3D)
vector as row * matrix -> vector
            1x3 * 3x3 -> 1x3

### Signature
```csharp
public static SearchAThing.Sci.Vector3D operator *(Vector3D v, Matrix3D m)
```

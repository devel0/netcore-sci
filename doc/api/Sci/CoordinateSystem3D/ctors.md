# SearchAThing.Sci.CoordinateSystem3D constructors
## CoordinateSystem3D(Vector3D, Vector3D, CoordinateSystem3DAutoEnum)
build coordinate system with given origin and given BaseZ on given vector normal;
            given normal will subjected to normalization;
            depending on csAutoType one or another arbitrary axis alghoritm will used to build cs from a point and a normal.

### Signature
```csharp
public CoordinateSystem3D(Vector3D o, Vector3D normal, CoordinateSystem3DAutoEnum csAutoType = 0)
```
### Parameters
- `o`: origin of cs
- `normal`: Z vector of cs
- `csAutoType`: auto cs type

### Remarks


<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## CoordinateSystem3D(Vector3D, Vector3D, Vector3D, Vector3D)
construct a coordinate system with the given origin and orthonormal bases
            note that given bases MUST already normalized

### Signature
```csharp
public CoordinateSystem3D(Vector3D o, Vector3D baseX, Vector3D baseY, Vector3D baseZ)
```
### Parameters
- `o`: cs origin
- `baseX`: cs X base ( must already normalized )
- `baseY`: cs Y base ( must already normalized )
- `baseZ`: cs Z base ( must already normalized )

### Remarks


<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## CoordinateSystem3D(Vector3D, Vector3D, Vector3D)
Construct a right-hand coordinate system with the given origin and bases such as:
            BaseX = v1
            BaseZ = v1 x BaseY
            BaseY = BaseZ x BaseX

### Signature
```csharp
public CoordinateSystem3D(Vector3D o, Vector3D v1, Vector3D v2)
```
### Remarks


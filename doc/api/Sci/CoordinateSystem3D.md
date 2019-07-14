# CoordinateSystem3D Class
**Namespace:** SearchAThing.Sci

**Inheritance:** Object → CoordinateSystem3D

(No Description)

## Signature
```csharp
public class CoordinateSystem3D
```
## Constructors
|**Name**|**Summary**|
|---|---|
|[CoordinateSystem3D(Vector3D, Vector3D, CoordinateSystem3DAutoEnum)](CoordinateSystem3D/ctors.md)||
|[CoordinateSystem3D(Vector3D, Vector3D, Vector3D, Vector3D)](CoordinateSystem3D/ctors.md#coordinatesystem3dvector3d-vector3d-vector3d-vector3d)|construct a coordinate system with the given origin and orthonormal bases<br/>            note that given bases MUST already normalized|
|[CoordinateSystem3D(Vector3D, Vector3D, Vector3D)](CoordinateSystem3D/ctors.md#coordinatesystem3dvector3d-vector3d-vector3d)|Construct a right-hand coordinate system with the given origin and bases such as:<br/>            BaseX = v1<br/>            BaseZ = v1 x BaseY<br/>            BaseY = BaseZ x BaseX|
## Methods
|**Name**|**Summary**|
|---|---|
|[Contains](CoordinateSystem3D/Contains.md)|verify if this cs XY plane contains given wcs point|
|[Equals](CoordinateSystem3D/Equals.md)||
|[Equals](CoordinateSystem3D/Equals.md#equalsdouble-coordinatesystem3d)|verify is this cs is equals to otherByLayer ( same origin, x, y, z base vectors )|
|[GetHashCode](CoordinateSystem3D/GetHashCode.md)||
|[GetType](CoordinateSystem3D/GetType.md)||
|[Intersect](CoordinateSystem3D/Intersect.md)|return intersect line between two cs xy planes|
|[IsParallelTo](CoordinateSystem3D/IsParallelTo.md)|states if this cs have Z base parallel to the other given cs|
|[Move](CoordinateSystem3D/Move.md)|return another cs with origin translated|
|[Rotate](CoordinateSystem3D/Rotate.md)|return another cs rotated respect given axis|
|[Rotate](CoordinateSystem3D/Rotate.md#rotatevector3d-double)|return another cs with same origin and base vector rotated about given vector|
|[ToCadString](CoordinateSystem3D/ToCadString.md)|script to paste in cad to draw cs rgb mode ( x=red, y=green, z=blue )|
|[ToString](CoordinateSystem3D/ToString.md)|debug string|
|[ToUCS](CoordinateSystem3D/ToUCS.md)|Transform wcs point to given cs|
|[ToWCS](CoordinateSystem3D/ToWCS.md)|transform ucs point to wcs|
## Properties
|**Name**|**Summary**|
|---|---|
|[BaseX](CoordinateSystem3D/BaseX.md)|cs x versor ( normalized )
|[BaseY](CoordinateSystem3D/BaseY.md)|cs y versor ( normalized )
|[BaseZ](CoordinateSystem3D/BaseZ.md)|cs z versor ( normalized )
|[CadScript](CoordinateSystem3D/CadScript.md)|script to paste in cad ( axis length = 1 )
|[Origin](CoordinateSystem3D/Origin.md)|origin of cs where x,y,z base vectors applied
## Fields
- [WCS](CoordinateSystem3D/WCS.md) (static)
- [XY](CoordinateSystem3D/XY.md) (static)
- [XZ](CoordinateSystem3D/XZ.md) (static)
- [YZ](CoordinateSystem3D/YZ.md) (static)
## Conversions

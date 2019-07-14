# Vector3D Class
**Namespace:** SearchAThing.Sci

**Inheritance:** Object → Geometry → Vector3D

can be used to describe a wcs point or a vector x,y,z components from some reference origin

## Signature
```csharp
public class Vector3D : SearchAThing.Sci.Geometry
```
## Constructors
|**Name**|**Summary**|
|---|---|
|[Vector3D()](Vector3D/ctors.md)|zero vector|
|[Vector3D(double[])](Vector3D/ctors.md#vector3ddouble)|build a vector (x,y,0) or (x,y,z) from given 2 or 3 doubles|
|[Vector3D(double, double, double)](Vector3D/ctors.md#vector3ddouble-double-double)|build a vector by given components|
|[Vector3D(double, double)](Vector3D/ctors.md#vector3ddouble-double)|build a vector (x,y,0) by given components|
|[Vector3D(string)](Vector3D/ctors.md#vector3dstring)|parse cad id string (eg. "X = 4.11641325 Y = 266.06066703 Z = 11.60392802")<br/>            constructing a point|
## Methods
|**Name**|**Summary**|
|---|---|
|[AngleRad](Vector3D/AngleRad.md)|angle between this and given vector|
|[AngleToward](Vector3D/AngleToward.md)|compute angle required to make this point go to the given one<br/>            if rotate right-hand around given reference axis|
|[Axis](Vector3D/Axis.md) (static)|retrieve wcs axis by given index|
|[BBox](Vector3D/BBox.md)|Compute bbox of this point.<br/>            ( Geometry BBox implementation ).|
|[Colinear](Vector3D/Colinear.md)|states if this vector is colinear to the given one|
|[Concordant](Vector3D/Concordant.md)|states if this vector concord to the given one<br/>            <br/>            **NOTE**: it does not test two vectors are parallels ( precondition must meet )|
|[ConcordantColinear](Vector3D/ConcordantColinear.md)|statis if this vector is concordant and colinear to the given one|
|[Convert](Vector3D/Convert.md)|convert each vector component value from to measure units|
|[Convert](Vector3D/Convert.md#convertmeasureunit-imudomain)|convert each vector component value from to measure units<br/>            to measure unit is given from the correspondent physical quantity measure unit of from mu|
|[Convert](Vector3D/Convert.md#convertimudomain-measureunit)|convert each vector component value from to measure units<br/>            from measure unit is given from the correspondent physical quantity measure unit of to mu|
|[CrossProduct](Vector3D/CrossProduct.md)|Cross product ( not normalized ) ;            <br/>            a x b = \|a\| \|b\| sin(alfa) N ;        <br/>            a x b = \|  x  y  z \|<br/>                    \| ax ay az \|<br/>                    \| bx by bz \|            <br/>            [reference](https://en.wikipedia.org/wiki/Cross_product) ;|
|[Distance](Vector3D/Distance.md)|compute distance between this point and the other given|
|[Distance](Vector3D/Distance.md#distancedouble-line3d)|compute perpendicular(min) distance of this point from given line|
|[Distance2D](Vector3D/Distance2D.md)|compute distance of this point from the given in 2d ( x,y ) without consider z component|
|[Divide](Vector3D/Divide.md)|Divide this point returning itself.<br/>            ( Geometry Divide implementation )|
|[DotProduct](Vector3D/DotProduct.md)|compute dot product of this vector for the given one            <br/>            a b = \|a\| \|b\| cos(alfa)|
|[Equals](Vector3D/Equals.md)||
|[EqualsAutoTol](Vector3D/EqualsAutoTol.md)|check if this vector equals the given one component by component using EqualsAutoTol|
|[EqualsTol](Vector3D/EqualsTol.md)|checks vector component equality vs other given|
|[EqualsTol](Vector3D/EqualsTol.md#equalstoldouble-double-double)|checks only x,y|
|[EqualsTol](Vector3D/EqualsTol.md#equalstoldouble-double-double-double)|checks vector component equality vs other given|
|[From2DCoords](Vector3D/From2DCoords.md) (static)|Create an array of Vector3D from given list of 2d coords ( eg. { 100, 200, 300, 400 }<br/>            will create follow list of vector3d = { (100,200,0), (300,400,0) }|
|[From3DCoords](Vector3D/From3DCoords.md) (static)|Create an array of Vector3D from given list of 3d coords ( eg. { 100, 200, 10, 300, 400, 20 }<br/>            will create follow list of vector3d = { (100,200,10), (300,400,20) }|
|[FromString](Vector3D/FromString.md) (static)|parse vector3d from string format "(x y z)" or "(x,y,z)" invariant type|
|[FromStringArray](Vector3D/FromStringArray.md) (static)|parse vector3d from array "(x1,y1,z1);(x2,y2,z2)"|
|[GetHashCode](Vector3D/GetHashCode.md)||
|[GetOrd](Vector3D/GetOrd.md)|retrieve the component (0:X, 1:Y, 2:Z)|
|[GetOrd](Vector3D/GetOrd.md#getordordidx)|retrieve the component (0:X, 1:Y, 2:Z)|
|[GetType](Vector3D/GetType.md)||
|[IsParallelTo](Vector3D/IsParallelTo.md)|Note: tol must be Constants.NormalizedLengthTolerance<br/>            if comparing normalized vectors|
|[IsPerpendicular](Vector3D/IsPerpendicular.md)|states is this vector is perpendicular to the given one|
|[Mirror](Vector3D/Mirror.md)|mirror this point about given axis|
|[Normalized](Vector3D/Normalized.md)|create a normalized version of this vector|
|[Project](Vector3D/Project.md)|project this vector to the given one|
|[Project](Vector3D/Project.md#projectline3d)|project this point to the given line|
|[Random](Vector3D/Random.md) (static)||
|[Random](Vector3D/Random.md#randomint-double-double-double-double-double-double-int-random) (static)|Span a set of qty vector3d with random coord between given range.<br/>            Optionally a seed can be specified for rand or Random obj directly ( in latter case seed aren't used )|
|[Rel](Vector3D/Rel.md)|create a vector relative to given origin from this point and given origin|
|[RotateAboutAxis](Vector3D/RotateAboutAxis.md)|rotate this point right-hand around given axis using quaternion|
|[RotateAboutAxis](Vector3D/RotateAboutAxis.md#rotateaboutaxisline3d-double)|rotate this point right-hand around given segment using quaternion|
|[RotateAboutXAxis](Vector3D/RotateAboutXAxis.md)|rotate this point around x-axis using quaternion|
|[RotateAboutYAxis](Vector3D/RotateAboutYAxis.md)|rotate this point around y-axis using quaternion|
|[RotateAboutZAxis](Vector3D/RotateAboutZAxis.md)|rotate this point around z-axis using quaternion|
|[RotateAs](Vector3D/RotateAs.md)|Note: tol must be Constants.NormalizedLengthTolerance<br/>            if comparing normalized vectors<br/>            rotation from-to will be multiplied for given angleFactor ( default 1.0 )|
|[Scalar](Vector3D/Scalar.md)|Scalar multiply each components|
|[ScaleAbout](Vector3D/ScaleAbout.md)|Scale this point about the given origin with the given factor.|
|[ScaleAbout](Vector3D/ScaleAbout.md#scaleaboutvector3d-vector3d)|Scale this point about the given origin with the given factor as (sx,sy,sz).|
|[Set](Vector3D/Set.md)|create a point copy of this one with component changed|
|[StringRepresentation](Vector3D/StringRepresentation.md)|string invariant representation "(x,y,z)"|
|[ToString](Vector3D/ToString.md)|string invariant representation "(x,y,z)"<br/>            w/3 decimal places|
|[ToString](Vector3D/ToString.md#tostringint)|string invariant representation "(x,y,z)" w/given digits|
|[ToString](Vector3D/ToString.md#tostringdouble)|hash string with given tolerance|
|[ToUCS](Vector3D/ToUCS.md)|Convert this wcs point to given cs coord|
|[ToWCS](Vector3D/ToWCS.md)|Convert this ucs considered vector using given cs to the wcs|
## Properties
|**Name**|**Summary**|
|---|---|
|[CadScript](Vector3D/CadScript.md)|cad script for this vector as wcs point
|[CadScriptLine](Vector3D/CadScriptLine.md)|cad script for a line (0,0,0) to this vector
|[CadScriptLineFrom](Vector3D/CadScriptLineFrom.md)|cad script for a line departing from this wcs point
|[Coordinates](Vector3D/Coordinates.md)|enumerate coordinates
|[DxfEntity](Vector3D/DxfEntity.md)|Create dxf point entity suitable for netDxf addEntity.<br/>            ( Geometry DxfEntity implementation )
|[GeomFrom](Vector3D/GeomFrom.md)|This vector.<br/>            ( Geometry GeomFrom implementation )
|[GeomTo](Vector3D/GeomTo.md)|This vector.<br/>            ( Geometry GeomTo implementation)
|[IsZeroLength](Vector3D/IsZeroLength.md)|states if this is a zero vector
|[Length](Vector3D/Length.md)|Length of this vector.<br/>            ( Geometry Length implementation )
|[Type](Vector3D/Type.md)|
|[Vertexes](Vector3D/Vertexes.md)|Enumerable with only this vector.<br/>            ( Geometry Vertexes implementation )
|[X](Vector3D/X.md)|X vector component
|[Y](Vector3D/Y.md)|Y vector component
|[Z](Vector3D/Z.md)|Z vector component
## Fields
- [XAxis](Vector3D/XAxis.md) (static)
- [YAxis](Vector3D/YAxis.md) (static)
- [ZAxis](Vector3D/ZAxis.md) (static)
- [Zero](Vector3D/Zero.md) (static)
## Operators
- [+](Vector3D/op_Addition.md)
- [-](Vector3D/op_UnaryNegation.md)
- [-](Vector3D/op_Subtraction.md)
- [*](Vector3D/op_Multiply.md)
- [*](Vector3D/op_Multiply.md)
- [*](Vector3D/op_Multiply.md)
- [/](Vector3D/op_Division.md)
- [/](Vector3D/op_Division.md)
## Conversions

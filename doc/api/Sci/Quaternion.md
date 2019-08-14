# Quaternion Class
**Namespace:** SearchAThing.Sci

**Inheritance:** Object → Quaternion

Quaternion implementation using doubles for purpose of Vector3D.RotateAboutAxis and Vector3D.RotateAs
            references:
            - http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/index.htm
            - https://www.3dgep.com/understanding-quaternions/
            - http://www.ncsa.illinois.edu/People/kindr/emtc/quaternions/

## Signature
```csharp
public class Quaternion
```
## Constructors
|**Name**|**Summary**|
|---|---|
|[Quaternion(double, Vector3D)](Quaternion/ctors.md)|direct construct quaternion q=[s, v]|
|[Quaternion(Vector3D, double)](Quaternion/ctors.md#quaternionvector3d-double)|build quaternion from axis and angle.<br/>            axis will be subjected to normalization.|
## Methods
|**Name**|**Summary**|
|---|---|
|[Conjugate](Quaternion/Conjugate.md)|Conjugate<br/>            q* = [s, -v]|
|[Equals](Quaternion/Equals.md)||
|[GetHashCode](Quaternion/GetHashCode.md)||
|[GetType](Quaternion/GetType.md)||
|[ToString](Quaternion/ToString.md)||
## Properties
|**Name**|**Summary**|
|---|---|
|[Identity](Quaternion/Identity.md) (static)|Identity qi = [1, nullvector]
|[v](Quaternion/v.md)|
## Operators
- [*](Quaternion/op_Multiply.md)
## Conversions

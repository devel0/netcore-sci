# SearchAThing.Sci.Vector3D constructors
## Vector3D()
zero vector

### Signature
```csharp
public Vector3D()
```
### Remarks
[unit test](/test/Vector3D/Vector3DTest_0006.cs)

<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## Vector3D(double[])
build a vector (x,y,0) or (x,y,z) from given 2 or 3 doubles

### Signature
```csharp
public Vector3D(double[] arr)
```
### Remarks
[unit test](/test/Vector3D/Vector3DTest_0007.cs)

<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## Vector3D(double, double, double)
build a vector by given components

### Signature
```csharp
public Vector3D(double x, double y, double z)
```
### Remarks
[unit test](/test/Vector3D/Vector3DTest_0008.cs)

<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## Vector3D(double, double)
build a vector (x,y,0) by given components

### Signature
```csharp
public Vector3D(double x, double y)
```
### Remarks
[unit test](/test/Vector3D/Vector3DTest_0008.cs)

<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## Vector3D(string)
parse cad id string (eg. "X = 4.11641325 Y = 266.06066703 Z = 11.60392802")
            constructing a point

### Signature
```csharp
public Vector3D(string cad_id_string)
```
### Parameters
- `cad_id_string`: cad id string

### Remarks
[unit test](/test/Vector3D/Vector3DTest_0009.cs)

# SearchAThing.Sci.Vector3D.Project method
## Project(Vector3D)
project this vector to the given one

### Signature
```csharp
public SearchAThing.Sci.Vector3D Project(Vector3D to)
```
### Parameters
- `to`: other vector

### Returns
projected vector ( will be colinear to the given one )
### Remarks
[unit test](/test/Vector3D/Vector3DTest_0021.cs)
            ![](/test/Vector3D/Vector3DTest_0021.png)

<p>&nbsp;</p>
<p>&nbsp;</p>
<hr/>

## Project(Line3D)
project this point to the given line

### Signature
```csharp
public SearchAThing.Sci.Vector3D Project(Line3D line)
```
### Parameters
- `line`: line to project the point onto

### Returns
projected point onto the line ( perpendicularly )
### Remarks


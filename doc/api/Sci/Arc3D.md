# Arc3D Class
**Namespace:** SearchAThing.Sci

**Inheritance:** Object → Geometry → Arc3D

base geometry for arc 3d entities

## Signature
```csharp
public class Arc3D : SearchAThing.Sci.Geometry
```
## Methods
|**Name**|**Summary**|
|---|---|
|[BBox](Arc3D/BBox.md)|compute wcs bbox executing a recursive bisect search of min and max|
|[Bulge](Arc3D/Bulge.md)|http://www.lee-mac.com/bulgeconversion.html|
|[CentreOfMass](Arc3D/CentreOfMass.md)||
|[CircleBy3Points](Arc3D/CircleBy3Points.md) (static)|helper to build circle by given 3 points|
|[Contains](Arc3D/Contains.md)|states if given point relies on this arc perimeter or shape depending on arguments|
|[Divide](Arc3D/Divide.md)|split arc into pieces and retrieve split points|
|[Equals](Arc3D/Equals.md)||
|[EqualsTol](Arc3D/EqualsTol.md)|Checks if two arcs are equals ( it checks agains swapped from-to too )|
|[GetHashCode](Arc3D/GetHashCode.md)||
|[GetType](Arc3D/GetType.md)||
|[Intersect](Arc3D/Intersect.md)|find ips of intersect this arc to the given cs plane; <br/>            return empty set if arc cs plane parallel to other given cs|
|[Intersect](Arc3D/Intersect.md#intersectdouble-line3d-bool-bool)|find ips of intersection between this arc and given line|
|[Move](Arc3D/Move.md)|create an arc copy with origin moved|
|[PtAngle](Arc3D/PtAngle.md)|return the angle (rad) of the point respect cs x axis rotating around cs z axis<br/>            to reach given point angle alignment|
|[PtAtAngle](Arc3D/PtAtAngle.md)|point on the arc circumnfere at given angle (rotating cs basex around cs basez)<br/>            note: it start|
|[Split](Arc3D/Split.md)|create a set of subarc from this by splitting through given split points<br/>            split point are not required to be on perimeter of the arc ( a center arc to point line will split )<br/>            generated subarcs will start from this arc angleFrom and contiguosly end to angleTo|
|[ToString](Arc3D/ToString.md)||
## Properties
- [Angle](Arc3D/Angle.md)
- [AngleEnd](Arc3D/AngleEnd.md)
- [AngleStart](Arc3D/AngleStart.md)
- [Center](Arc3D/Center.md)
- [CS](Arc3D/CS.md)
- [DxfEntity](Arc3D/DxfEntity.md)
- [From](Arc3D/From.md)
- [GeomFrom](Arc3D/GeomFrom.md)
- [GeomTo](Arc3D/GeomTo.md)
- [Length](Arc3D/Length.md)
- [MidPoint](Arc3D/MidPoint.md)
- [Radius](Arc3D/Radius.md)
- [Segment](Arc3D/Segment.md)
- [To](Arc3D/To.md)
- [Type](Arc3D/Type.md)
- [Vertexes](Arc3D/Vertexes.md)
## Conversions

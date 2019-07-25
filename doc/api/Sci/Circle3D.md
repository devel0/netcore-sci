# Circle3D Class
**Namespace:** SearchAThing.Sci

**Inheritance:** Object → Geometry → Arc3D → Circle3D

(No Description)

## Signature
```csharp
public class Circle3D : SearchAThing.Sci.Arc3D
```
## Constructors
|**Name**|**Summary**|
|---|---|
|[Circle3D(double, CoordinateSystem3D, double)](Circle3D/ctors.md)||
|[Circle3D(Arc3D)](Circle3D/ctors.md#circle3darc3d)|create circle from given arc|
|[Circle3D(Vector3D, Vector3D, Vector3D)](Circle3D/ctors.md#circle3dvector3d-vector3d-vector3d)|Build 3d circle that intersect p1,p2,p3<br/>            ( the inside CS will centered in the circle center and Xaxis toward p1 )|
## Methods
|**Name**|**Summary**|
|---|---|
|[BBox](Circle3D/BBox.md)|compute wcs bbox executing a recursive bisect search of min and max|
|[Bulge](Circle3D/Bulge.md)|http://www.lee-mac.com/bulgeconversion.html|
|[CentreOfMass](Circle3D/CentreOfMass.md)||
|[CircleRTanP](Circle3D/CircleRTanP.md) (static)|build 3d circle through point p, tangent to given t line, with given radius r            <br/>            they can be two|
|[CirclesTan12P](Circle3D/CirclesTan12P.md) (static)|build 3d circle that tangent to lines t1,t2 and that intersects point p<br/>            note: point p must contained in one of t1,t2<br/>            circle will be inside region t1.V toward t2.V<br/>            they are 4 circles|
|[Contains](Circle3D/Contains.md)||
|[Divide](Circle3D/Divide.md)|split arc into pieces and retrieve split points|
|[Equals](Circle3D/Equals.md)||
|[EqualsTol](Circle3D/EqualsTol.md)|Checks if two arcs are equals ( it checks agains swapped from-to too )|
|[GetHashCode](Circle3D/GetHashCode.md)||
|[GetType](Circle3D/GetType.md)||
|[Intersect](Circle3D/Intersect.md)|find ips of intersect this arc to the given cs plane; <br/>            return empty set if arc cs plane parallel to other given cs|
|[Intersect](Circle3D/Intersect.md#intersectdouble-line3d-bool-bool)|intersect this 3d circle with given 3d line|
|[Move](Circle3D/Move.md)|create an arc copy with origin moved|
|[PtAngle](Circle3D/PtAngle.md)|return the angle (rad) of the point respect cs x axis rotating around cs z axis<br/>            to reach given point angle alignment|
|[PtAtAngle](Circle3D/PtAtAngle.md)|point on the arc circumnfere at given angle (rotating cs basex around cs basez)<br/>            note: it start|
|[Split](Circle3D/Split.md)|create a set of subarc from this by splitting through given split points<br/>            split point are not required to be on perimeter of the arc ( a center arc to point line will split )<br/>            generated subarcs will start from this arc angleFrom and contiguosly end to angleTo|
|[ToPolygon3D](Circle3D/ToPolygon3D.md)|creates a polygon approximation of this circle with segments of given maxLength|
|[ToString](Circle3D/ToString.md)||
## Properties
|**Name**|**Summary**|
|---|---|
|[Angle](Circle3D/Angle.md)|Arc (rad) angle length.<br/>            angle between start-end or end-start depending on what start is less than end or not
|[AngleEnd](Circle3D/AngleEnd.md)|end angle (rad) [0-2pi) respect cs xaxis rotating around cs zaxis<br/>            note that start angle can be greather than end angle
|[AngleStart](Circle3D/AngleStart.md)|start angle (rad) [0-2pi) respect cs xaxis rotating around cs zaxis<br/>            note that start angle can be greather than end angle
|[Area](Circle3D/Area.md)|
|[Center](Circle3D/Center.md)|
|[CS](Circle3D/CS.md)|coordinate system centered in arc center<br/>            angle is 0 at X axis<br/>            angle increase rotating right-hand on Z axis
|[DxfEntity](Circle3D/DxfEntity.md)|
|[From](Circle3D/From.md)|point at angle start
|[GeomFrom](Circle3D/GeomFrom.md)|
|[GeomTo](Circle3D/GeomTo.md)|
|[Length](Circle3D/Length.md)|
|[MidPoint](Circle3D/MidPoint.md)|mid point eval as arc point at angle start + arc angle/2
|[Radius](Circle3D/Radius.md)|radius of arc
|[Segment](Circle3D/Segment.md)|return From,To segment
|[To](Circle3D/To.md)|point at angle end
|[Type](Circle3D/Type.md)|
|[Vertexes](Circle3D/Vertexes.md)|
## Conversions

# Line3D Class
**Namespace:** SearchAThing.Sci

**Inheritance:** Object → Geometry → Line3D

(No Description)

## Signature
```csharp
public class Line3D : SearchAThing.Sci.Geometry
```
## Methods
|**Name**|**Summary**|
|---|---|
|[ApparentIntersect](Line3D/ApparentIntersect.md)|Find apparent intersection between this and given other line<br/>            returning (shortest) segment perpendicular to either lines or null if lines parallels.<br/>            This method will used from Intersect to find intersection between lines when<br/>            perpendicular segment length not exceed given length tolerance.|
|[BBox](Line3D/BBox.md)||
|[Bisect](Line3D/Bisect.md)|returns bisect of two given segment/lines<br/>            ( if given segment not share nodes but intesects returned bisect start from ip )<br/>            bisect choosen will be the one between this and other withing shortest angle<br/>            <br/>            if two given lines are parallel and parallelRotationAxis is given then<br/>            bisect results as this segment rotated PI/2 about given axis using To as rotcenter|
|[Colinear](Line3D/Colinear.md)||
|[CommonPoint](Line3D/CommonPoint.md)|returns the common point from,to between two lines or null if not consecutives|
|[Divide](Line3D/Divide.md)||
|[EnsureFrom](Line3D/EnsureFrom.md)|if this segment from matches the given point returns this;<br/>            if this segment to matches the given point return this with from,to swapped;<br/>            precondition: this segment must have from or to equals given from|
|[Equals](Line3D/Equals.md)||
|[EqualsTol](Line3D/EqualsTol.md)|Checks if two lines are equals ( it checks agains swapped from-to too )|
|[GetHashCode](Line3D/GetHashCode.md)||
|[GetType](Line3D/GetType.md)||
|[Intersect](Line3D/Intersect.md)|returns null if this line is parallel to the cs xy plane,<br/>            the intersection point otherwise|
|[Intersect](Line3D/Intersect.md#intersectdouble-plane3d)|returns null if this line is parallel to the plane,<br/>            the intersection point otherwise|
|[Intersect](Line3D/Intersect.md#intersectdouble-line3d-lineintersectbehavior)|Find intersection point between this and other line using given tolerance.<br/>            Returns null if no intersection, otherwise it returns a point on<br/>            the shortest segment ( the one that's perpendicular to either lines )<br/>            based on given behavior ( default midpoint ).|
|[Intersect](Line3D/Intersect.md#intersectdouble-line3d-bool-bool)|Intersects two lines with arbitrary segment mode for each.|
|[IsParallelTo](Line3D/IsParallelTo.md)||
|[IsParallelTo](Line3D/IsParallelTo.md#isparalleltodouble-plane3d)||
|[LineContainsPoint](Line3D/LineContainsPoint.md)|Infinite line contains point.|
|[LineContainsPoint](Line3D/LineContainsPoint.md#linecontainspointdouble-double-double-double-bool)|Infinite line contains point.<br/>            Note: tol must be Constant.NormalizedLengthTolerance<br/>            if comparing normalized vectors|
|[Move](Line3D/Move.md)|move this segment of given delta|
|[MoveMidpoint](Line3D/MoveMidpoint.md)|Move this segment midpoint to the given coord|
|[Normalized](Line3D/Normalized.md)|build a segment with same from and vector normalized|
|[Offset](Line3D/Offset.md)|create offseted line toward refPt for given offset|
|[Perpendicular](Line3D/Perpendicular.md)|Build a perpendicular vector to this one starting from the given point p.|
|[Reverse](Line3D/Reverse.md)|return the segment with swapped from,to|
|[RotateAboutAxis](Line3D/RotateAboutAxis.md)|rotate this segment about given axis|
|[Scale](Line3D/Scale.md)|scale from,to of this line using given refpt and factor|
|[SegmentContainsPoint](Line3D/SegmentContainsPoint.md)|Finite segment contains point.<br/>            Note: tol must be Constant.NormalizedLengthTolerance<br/>            if comparing normalized vectors|
|[SegmentContainsPoint](Line3D/SegmentContainsPoint.md#segmentcontainspointdouble-double-double-double)|Finite segment contains point.<br/>            Note: tol must be Constant.NormalizedLengthTolerance<br/>            if comparing normalized vectors|
|[SemiLineContainsPoints](Line3D/SemiLineContainsPoints.md)|states if semiline From-To(inf) contains given point|
|[SetLength](Line3D/SetLength.md)|resize this segment to a new one with same From|
|[Split](Line3D/Split.md)|split current segment into one or more depending on which of given split points was found on the segment            <br/>            splitted segments start from begin of line<br/>            TODO : not optimized|
|[ToString](Line3D/ToString.md)|build an invariant string representation w/3 digits<br/>            (f.x, f.y, f.z)-(t.x, t.y, t.z) L=len Δ=(v.x, v.y, v.z)|
|[ToString](Line3D/ToString.md#tostringdouble)|hash string with given tolerance|
|[ToString](Line3D/ToString.md#tostringint)|build an invariant string representation w/given digits<br/>            (f.x, f.y, f.z)-(t.x, t.y, t.z) L=len Δ=(v.x, v.y, v.z)|
## Properties
|**Name**|**Summary**|
|---|---|
|[CadScript](Line3D/CadScript.md)|
|[Dir](Line3D/Dir.md)|V normalized
|[DisambiguatedPoints](Line3D/DisambiguatedPoints.md)|retrieve a unique endpoint representation of this line3d segment (regardless its from-to or to-from order)<br/>            such that From.Distance(Vector3D.Zero) less than To.Distance(Vector3D.Zero)
|[DxfEntity](Line3D/DxfEntity.md)|
|[From](Line3D/From.md)|application point vector
|[GeomFrom](Line3D/GeomFrom.md)|
|[GeomTo](Line3D/GeomTo.md)|
|[Inverted](Line3D/Inverted.md)|return inverted segment
|[Length](Line3D/Length.md)|
|[MidPoint](Line3D/MidPoint.md)|
|[Points](Line3D/Points.md)|
|[Swapped](Line3D/Swapped.md)|return segment with swapped from,to
|[To](Line3D/To.md)|From + V
|[Type](Line3D/Type.md)|
|[V](Line3D/V.md)|vector depart at From to identify To
|[Vertexes](Line3D/Vertexes.md)|
## Fields
- [XAxisLine](Line3D/XAxisLine.md) (static)
- [YAxisLine](Line3D/YAxisLine.md) (static)
- [ZAxisLine](Line3D/ZAxisLine.md) (static)
## Operators
- [*](Line3D/op_Multiply.md)
- [*](Line3D/op_Multiply.md)
- [+](Line3D/op_Addition.md)
- [-](Line3D/op_Subtraction.md)
## Conversions

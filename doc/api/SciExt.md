# SciExt Class
**Namespace:** SearchAThing

**Inheritance:** Object → SciExt

(No Description)

## Signature
```csharp
public static class SciExt
```
## Methods
|**Name**|**Summary**|
|---|---|
|[AddEntities](SciExt/AddEntities.md) (static)|add entity to the given dxf object ( it can be Dxfdocument or Block )<br/>            optionally set layer|
|[AddEntity](SciExt/AddEntity.md) (static)|add entity to the given dxf object ( it can be Dxfdocument or Block )<br/>            optionally set layer|
|[Angle](SciExt/Angle.md) (static)|retrieve angle between from and to given;<br/>            angles will subjected to normalization [0,2pi) and angle from can be greather than to|
|[AngleInRange](SciExt/AngleInRange.md) (static)|states if given angle is contained in from, to angle range;<br/>            multiturn angles are supported because test will normalize to [0,2pi) automatically.|
|[Area](SciExt/Area.md) (static)|Area of a polygon (does not consider z)<br/>            https://en.wikipedia.org/wiki/Centroid|
|[AutoIntersect](SciExt/AutoIntersect.md) (static)|autointersect given list of segments<br/>            ( duplicates and overlapping are removed )<br/>            <br/>            TODO: dummy function, optimize|
|[AutoZoom](SciExt/AutoZoom.md) (static)|tries to zoom dxf viewport on the given bbox|
|[BBox](SciExt/BBox.md) (static)||
|[BBox](SciExt/BBox.md#bboxentityobject-double) (static)||
|[BBox](SciExt/BBox.md#bboxienumerablenetdxfentitiesentityobject-double) (static)||
|[BBox](SciExt/BBox.md#bboxienumerablesearchathingscigeometry-double) (static)||
|[Boolean](SciExt/Boolean.md) (static)|can generate a Int64MapExceptionRange exception if double values can't fit into a In64 representation.<br/>            In that case try with tolerances not too small.<br/>            It is suggested to use a lenTol/10 to avoid lost of precision during domain conversions.|
|[ByPhysicalQuantity](SciExt/ByPhysicalQuantity.md) (static)||
|[CadScript](SciExt/CadScript.md) (static)||
|[CadScript](SciExt/CadScript.md#cadscriptface3d) (static)||
|[CadScriptPoint](SciExt/CadScriptPoint.md) (static)||
|[CadScriptPolyline](SciExt/CadScriptPolyline.md) (static)||
|[Center](SciExt/Center.md) (static)|Same as mean|
|[Centroid](SciExt/Centroid.md) (static)|Centroid of a polygon (does not consider z)<br/>            note: points must ordered anticlockwise<br/>            ( if have area specify the parameter to avoid recomputation )<br/>            https://en.wikipedia.org/wiki/Centroid|
|[Centroid](SciExt/Centroid.md#centroidireadonlylistsearchathingscivector3d-double-double) (static)|Centroid of a polygon (does not consider z)<br/>            note: points must ordered anticlockwise<br/>            https://en.wikipedia.org/wiki/Centroid|
|[CircleBy3Points](SciExt/CircleBy3Points.md) (static)||
|[ClosedPolys2D](SciExt/ClosedPolys2D.md) (static)|build polygons from given list of segments<br/>            if want to represent arcs, add them as dummy lines to segs<br/>            polys returned are ordered anticlockwise|
|[ContainsPoint](SciExt/ContainsPoint.md) (static)|states if the given polygon contains the test point ( z not considered )<br/>            https://en.wikipedia.org/wiki/Point_in_polygon<br/>            By default check the point contained in the polygon perimeter.<br/>            Optionally duplicate points are zapped in comparing.|
|[Convert](SciExt/Convert.md) (static)|convert nullable double from to measure units|
|[Convert](SciExt/Convert.md#convertnullabledouble-imudomain-measureunit) (static)|convert nullable double from to measure units|
|[Convert](SciExt/Convert.md#convertnullabledouble-measureunit-imudomain) (static)|convert nullable double from to measure units|
|[Convert](SciExt/Convert.md#convertdouble-measureunit-measureunit) (static)|convert given value from to measure units|
|[Convert](SciExt/Convert.md#convertdouble-measureunit-imudomain) (static)|convert given value from to measure units<br/>            to measure unit is given from the correspondent physical quantity measure unit of from mu|
|[Convert](SciExt/Convert.md#convertdouble-imudomain-measureunit) (static)|convert given value from to measure units<br/>            from measure unit is given from the correspondent physical quantity measure unit of to mu|
|[ConvertToMeasure](SciExt/ConvertToMeasure.md) (static)|convert given value from the given measure unit in the domain corresponding to the physical quantity of given to<br/>            and build a measure with given to measure unit|
|[ConvexHull2D](SciExt/ConvexHull2D.md) (static)|compute convex hull using LoycCore<br/>            https://github.com/qwertie/LoycCore|
|[CoordTransform](SciExt/CoordTransform.md) (static)||
|[CoordTransform](SciExt/CoordTransform.md#coordtransformentityobject-funcsearchathingscivector3d-searchathingscivector3d-vector3d) (static)|build a clone of the given entity with coord transformed accordingly given function.|
|[DistinctKeepOrder](SciExt/DistinctKeepOrder.md) (static)|retrieve distinct of given vector set ensuring to maintain given order|
|[DrawCube](SciExt/DrawCube.md) (static)|Creates and add dxf entities for a 6 faces of a cube|
|[DrawCuboid](SciExt/DrawCuboid.md) (static)||
|[DrawCuboid](SciExt/DrawCuboid.md#drawcuboiddxfobject-vector3d-vector3d-layer) (static)|Creates and add dxf entities for 6 faces of a cuboid|
|[DrawStar](SciExt/DrawStar.md) (static)|Creates and add dxf entities for a 3 axis of given length centered in given center point.|
|[DrawTimeline](SciExt/DrawTimeline.md) (static)||
|[Equals](SciExt/Equals.md)||
|[EqualsTol](SciExt/EqualsTol.md) (static)|checks two list of vectors are equals and with same order of elements|
|[EqualsTol](SciExt/EqualsTol.md#equalstolienumerablesystemvaluetupledouble-double-ienumerablesystemvaluetupledouble-double-double-double) (static)|compares two list tuples|
|[Explode](SciExt/Explode.md) (static)||
|[GeomCentroid](SciExt/GeomCentroid.md) (static)||
|[GetHashCode](SciExt/GetHashCode.md)||
|[GetType](SciExt/GetType.md)||
|[Intersect](SciExt/Intersect.md) (static)|Find intersection points (0,1,2) of the given line with the given polygon<br/>            TODO unit test|
|[IsAClosedPoly](SciExt/IsAClosedPoly.md) (static)|Preprocess segs with SortPoly if needed.<br/>            Return the ordered segments poly or null if not a closed poly.|
|[IsLinearIndependent](SciExt/IsLinearIndependent.md) (static)|states if given 3 vectors are linearly independent<br/>            [unit test](/test/Vector3D/Vector3DTest_0001.cs)|
|[Length](SciExt/Length.md) (static)|compute length of polyline from given seq_pts|
|[Mean](SciExt/Mean.md) (static)|mean of given vetor3d list<br/>            note: if used to compute poly center enable skipFirstAtEnd|
|[MergeColinearSegments](SciExt/MergeColinearSegments.md) (static)|merge colinear overlapped segments into single<br/>            result segments direction and order is not ensured<br/>            pre: segs must colinear|
|[MidPoint](SciExt/MidPoint.md) (static)|get the midpoint of the 3d polyline<br/>            distance is computed over all segments|
|[MinDistance](SciExt/MinDistance.md) (static)|Return the min distance between two adiacent number<br/>            given from all of the given ordered set of numbers.|
|[MinMax](SciExt/MinMax.md) (static)|retrieve min,max w/single sweep|
|[NormalizeAngle2PI](SciExt/NormalizeAngle2PI.md) (static)|ensure given angle in [0,2*PI] range|
|[Offset](SciExt/Offset.md) (static)|increase of decrease polygon points offseting|
|[Points](SciExt/Points.md) (static)||
|[PolygonSegments](SciExt/PolygonSegments.md) (static)|yields an ienumerable of polygon segments corresponding to the given polygon pts ( z is not considered )<br/>            works even last point not equals the first one|
|[PolyPoints](SciExt/PolyPoints.md) (static)|retrieve s[0].from, s[1].from, ... s[n-1].from, s[n-1].to points|
|[PolyPoints](SciExt/PolyPoints.md#polypointsienumerablesearchathingscivector3d-double-bool) (static)|given a set of polygon pts, returns the enumeation of all pts<br/>            so that the last not attach to the first ( if makeClosed = false ).<br/>            Elsewhere it returns a last point equals the first ( makeClosed = true ).|
|[Project](SciExt/Project.md) (static)|wcs coord of projected coord to the given cs|
|[RadTol](SciExt/RadTol.md) (static)|compute angle rad tolerance by given arc length tolerance|
|[RepeatFirstAtEnd](SciExt/RepeatFirstAtEnd.md) (static)|given points a,b,c it will return a,b,c,a ( first is repeated at end )<br/>            it avoid to repeat first at end when latest point already equals the first one|
|[Segments](SciExt/Segments.md) (static)|segments representation of given geometries<br/>            if arc found a segment between endpoints returns|
|[SetColor](SciExt/SetColor.md) (static)||
|[SetLayer](SciExt/SetLayer.md) (static)||
|[SetLayer](SciExt/SetLayer.md#setlayerienumerablenetdxfentitiesentityobject-layer) (static)|Set layer of given set of dxf entities|
|[SortPoly](SciExt/SortPoly.md) (static)||
|[SortPoly](SciExt/SortPoly.md#sortpolyienumerablesearchathingsciline3d-double-vector3d) (static)|Sort polygon segments so that they can form a polygon ( if they really form one ).<br/>            It will not check for segment versus adjancency|
|[SortPoly](SciExt/SortPoly.md#sortpolytienumerablet-double-funct-searchathingscivector3d-vector3d) (static)||
|[StringRepresentation](SciExt/StringRepresentation.md) (static)|array invariant string vector3d representation "(x1,y1,z2);(x2,y2,z2)"|
|[Sum](SciExt/Sum.md) (static)||
|[TakeUntilAdjacent](SciExt/TakeUntilAdjacent.md) (static)|Return the input set of segments until an adjacency between one and next is found.<br/>            It can rectify the versus of line (by default) if needed.<br/>            Note: returned set references can be different if rectifyVersus==true|
|[Thin](SciExt/Thin.md) (static)|retrieve given input set ordered with only distinct values after comparing through tolerance<br/>            in this case result set contains only values from the input set (default) or rounding to given tol if maintain_original_values is false;<br/>            if keep_ends true (default) min and max already exists at begin/end of returned sequence|
|[ToArc3D](SciExt/ToArc3D.md) (static)||
|[ToCadScript](SciExt/ToCadScript.md) (static)||
|[ToCircle3D](SciExt/ToCircle3D.md) (static)||
|[ToCoordSequence](SciExt/ToCoordSequence.md) (static)|from a list of vector3d retrieve x1,y1,z1,x2,y2,z2,... coord sequence|
|[ToCoordString2D](SciExt/ToCoordString2D.md) (static)|produce a string with x1,y1,x2,y2, ...|
|[ToCoordString3D](SciExt/ToCoordString3D.md) (static)|produce a string with x1,y1,z1,x2,y2,z2, ...|
|[ToCSV](SciExt/ToCSV.md) (static)|exports to a csv string some known fields<br/>            note: not really a csv its a tab separated values for debug purpose<br/>            just copy and paste|
|[ToDxfPoint](SciExt/ToDxfPoint.md) (static)|create dxf point from given vector3d|
|[ToDxfUCS](SciExt/ToDxfUCS.md) (static)||
|[ToFace3DList](SciExt/ToFace3DList.md) (static)||
|[ToGeometryBlock](SciExt/ToGeometryBlock.md) (static)||
|[ToLine](SciExt/ToLine.md) (static)||
|[ToLine3D](SciExt/ToLine3D.md) (static)||
|[ToLwPolyline](SciExt/ToLwPolyline.md) (static)|build 2d dxf polyline.<br/>            note: use RepeatFirstAtEnd extension to build a closed polyline|
|[ToPoint](SciExt/ToPoint.md) (static)|To point (double x, double y)|
|[ToPolyline](SciExt/ToPolyline.md) (static)|build 3d dxf polyline<br/>            note: use RepeatFirstAtEnd extension to build a closed polyline|
|[ToPsql](SciExt/ToPsql.md) (static)|creates a psql double[] string|
|[ToPsql](SciExt/ToPsql.md#topsqlienumerablesearchathingscivector3d) (static)|create a psql representation of double[] coord sequence x1,y1,z1,x2,y2,z2, ... of given points|
|[ToString](SciExt/ToString.md)||
|[ToVector2](SciExt/ToVector2.md) (static)||
|[ToVector3D](SciExt/ToVector3D.md) (static)||
|[Vector3DCoords](SciExt/Vector3DCoords.md) (static)|enumerate as Vector3D given dxf polyline vertexes|
|[Vector3DCoords](SciExt/Vector3DCoords.md#vector3dcoordslwpolyline) (static)|enumerate as Vector3D given dxf lwpolyline vertexes|
|[Vertexes](SciExt/Vertexes.md) (static)||
|[WeightedDistribution](SciExt/WeightedDistribution.md) (static)|retrieve a list of N pairs (value,presence)<br/>             with value between min and max of inputs and presence between 0..1 that represents the percent of presence of the value<br/>             <br/>             examples:<br/>            <br/>             inputs = ( 1, 2, 3 ), N = 3<br/>             results: ( (1, .33), (2, .33), (3, .33) )<br/>             <br/>             inputs = ( 1, 2.49, 3), N = 3<br/>             results: ( (1, .33), (2, .169), (3, .497) )<br/>             <br/>             inputs = ( 1, 2, 3), N = 4<br/>             results: ( (1, .33), (1.6, .16), (2.3, .16), (3, .33) )|
|[ZapDuplicates](SciExt/ZapDuplicates.md) (static)|return pts (maintaining order) w/out duplicates<br/>            use the other overloaded method if already have a vector 3d equality comparer|
|[ZapDuplicates](SciExt/ZapDuplicates.md#zapduplicatesienumerablesearchathingscivector3d-vector3dequalitycomparer) (static)|return pts (maintaining order) w/out duplicates|
## Conversions

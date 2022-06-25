using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using netDxf.Entities;
using netDxf;
using netDxf.Blocks;
using netDxf.Tables;

using SearchAThing;
using static SearchAThing.SciToolkit;

namespace SearchAThing
{

    public static partial class SciToolkit
    {

        public static Func<string, string> PostProcessCadScript = (script) =>
             script.Replace("\r\n", "\n");

        /// <summary>
        /// Creates dxf entities for a 3 axis of given length centered in given center point.
        /// </summary>        
        public static IEnumerable<Line> Star(Vector3D center, double L)
        {
            yield return new Line((center - L / 2 * Vector3D.XAxis), (center + L / 2 * Vector3D.XAxis));
            yield return new Line((center - L / 2 * Vector3D.YAxis), (center + L / 2 * Vector3D.YAxis));
            yield return new Line((center - L / 2 * Vector3D.ZAxis), (center + L / 2 * Vector3D.ZAxis));
        }

        /// <summary>
        /// Creates dxf entities for a 6 faces of a cube
        /// </summary>        
        public static IEnumerable<Face3d> Cube(Vector3D center, double L) => Cuboid(center, new Vector3D(L, L, L));

        /// <summary>
        /// Creates dxf entities for 6 faces of a cuboid
        /// </summary>        
        public static IEnumerable<Face3d> Cuboid(Vector3D center, Vector3D size)
        {
            var corner = center - size / 2;

            // is this a cuboid ? :)
            //
            //       011------------111
            //      / .            / |
            //   001------------101  |      z
            //    |   .          |   |      |    y
            //    |   .          |   |      |  /
            //    |  010.........|. 110     | /
            //    | .            | /        |/
            //   000------------100         ---------x
            //
            var m = new Vector3[2, 2, 2];
            for (int xi = 0; xi < 2; ++xi)
            {
                for (int yi = 0; yi < 2; ++yi)
                {
                    for (int zi = 0; zi < 2; ++zi)
                    {
                        m[xi, yi, zi] = (corner + size.Scalar(xi, yi, zi));
                    }
                }
            }

            yield return new Face3d(m[0, 0, 0], m[1, 0, 0], m[1, 0, 1], m[0, 0, 1]); // front
            yield return new Face3d(m[0, 1, 0], m[0, 1, 1], m[1, 1, 1], m[1, 1, 0]); // back
            yield return new Face3d(m[0, 0, 0], m[0, 0, 1], m[0, 1, 1], m[0, 1, 0]); // left
            yield return new Face3d(m[1, 0, 0], m[1, 1, 0], m[1, 1, 1], m[1, 0, 1]); // right
            yield return new Face3d(m[0, 0, 0], m[0, 1, 0], m[1, 1, 0], m[1, 0, 0]); // bottom
            yield return new Face3d(m[0, 0, 1], m[1, 0, 1], m[1, 1, 1], m[0, 1, 1]); // top
        }

    }

    public static partial class SciExt
    {

        public static EntityObject SetLayer(this EntityObject eo, Layer layer)
        {
            eo.Layer = layer;
            return eo;
        }

        /// <summary>
        /// get the midpoint of the 3d polyline
        /// distance is computed over all segments
        /// </summary>        
        public static Vector3D? MidPoint(this Polyline poly)
        {
            var mid_len = poly.Vector3DCoords().Length() / 2;
            Vector3D? prev = null;
            var pos = 0.0;
            var en = poly.Vector3DCoords().GetEnumerator();
            while (en.MoveNext())
            {
                if (prev == null)
                    prev = en.Current;
                else
                {
                    var prev_cur_dst = en.Current.Distance(prev);
                    if (pos + prev_cur_dst >= mid_len)
                    {
                        // mid is between prev and current
                        var leftLen = mid_len - pos;
                        return prev + (en.Current - prev).Normalized() * leftLen;
                    }
                    pos += prev_cur_dst;
                    prev = en.Current;
                }
            }
            return null;
        }

        public static IEnumerable<EntityObject> Explode(this Insert ins)
        {
            var insPt = ins.Position;

            var N = ins.Normal;
            var ocs = new CoordinateSystem3D(insPt, N).Rotate(N, ins.Rotation.ToRad());
            var origin = Vector3D.Zero.ToWCS(ocs);

            foreach (var ent in ins.Block.Entities)
            {
                // TODO scale
                // pts = pts.Select(w => w.ScaleAbout(Vector3D.Zero, ins.Scale.ToVector3D()));


                // pts = pts.Select(w => w.ToWCS(ocs));

                switch (ent.Type)
                {
                    case EntityType.Circle:
                        {
                            var c = (Circle)ent.CoordTransform((x) => x.ToWCS(ocs), origin);
                            c.Center = (c.Center + insPt);
                            yield return c;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// enumerate as Vector3D given dxf polyline vertexes
        /// </summary>        
        public static IEnumerable<Vector3D> Vector3DCoords(this Polyline pl) =>
            pl.Vertexes.Select(w => (Vector3D)w.Position);

        /// <summary>
        /// enumerate as Vector3D given dxf lwpolyline vertexes
        /// </summary>        
        public static IEnumerable<Vector3D> Vector3DCoords(this LwPolyline lwp)
        {
            var res = new List<Vector3D>();
            var N = lwp.Normal;
            var ocs = new CoordinateSystem3D(Vector3D.Zero, N);

            foreach (var v in lwp.Vertexes)
            {
                yield return MathHelper.Transform(
                    new Vector3(v.Position.X, v.Position.Y, lwp.Elevation), lwp.Normal, CoordinateSystem.Object, CoordinateSystem.World);
            }
        }

        /// <summary>
        /// given points a,b,c it will return a,b,c,a ( first is repeated at end )
        /// it avoid to repeat first at end when latest point already equals the first one
        /// </summary>        
        public static IEnumerable<Vector3D> RepeatFirstAtEnd(this IEnumerable<Vector3D> pts, double tol)
        {
            Vector3D? first = null;
            Vector3D? last = null;
            foreach (var x in pts)
            {
                if (first == null) first = x;
                last = x;
                yield return x;
            }

            if (last == null) yield break;

            if (first != null && !last.EqualsTol(tol, first)) yield return first;
        }

        public static IEnumerable<EntityObject> CoordTransform(this DxfDocument dxf, Func<Vector3D, Vector3D> transform)
        {
            foreach (var point in dxf.Points) yield return point.CoordTransform(transform);
            foreach (var line in dxf.Lines) yield return line.CoordTransform(transform);
            foreach (var lwpoly in dxf.LwPolylines) yield return lwpoly.CoordTransform(transform);
            foreach (var poly in dxf.Polylines) yield return poly.CoordTransform(transform);
            foreach (var circle in dxf.Circles) yield return circle.CoordTransform(transform);
            foreach (var text in dxf.Texts) yield return text.CoordTransform(transform);
            foreach (var mtext in dxf.MTexts) yield return mtext.CoordTransform(transform);

            var origin = transform(Vector3D.Zero);
            var insBlocks = dxf.Inserts.Select(w => w.Block).Distinct();
            Dictionary<string, Block> blkDict = new Dictionary<string, Block>();
            foreach (var _insBlock in insBlocks)
            {
                var insBlock = (Block)_insBlock.Clone();
                var ents = insBlock.Entities.ToList();
                insBlock.Entities.Clear();
                foreach (var x in ents)
                {
                    if (x.Type == EntityType.Hatch) continue; // TODO hatch
                    insBlock.Entities.Add(x.CoordTransform(transform, origin));
                }
                blkDict.Add(insBlock.Name, insBlock);
            }

            foreach (var _ins in dxf.Inserts)
            {
                var ins = (Insert)_ins.Clone();//.Clone(blkDict[_ins.Block.Name]);
                                               //ins.Position = transform(ins.Position);
                yield return ins;
            }
        }

        /// <summary>
        /// build a clone of the given entity with coord transformed accordingly given function.
        /// </summary>        
        public static EntityObject CoordTransform(this EntityObject eo, Func<Vector3D, Vector3D> transform, Vector3D? origin = null)
        {
            switch (eo.Type)
            {
                case EntityType.Insert:
                    {
                        var ins = (Insert)eo.Clone();
                        ins.Position = transform(ins.Position);
                        return ins;
                    }

                case EntityType.Line:
                    {
                        var line = (Line)eo.Clone();
                        line.StartPoint = transform(line.StartPoint);
                        line.EndPoint = transform(line.EndPoint);
                        return line;
                    }

                case EntityType.Text:
                    {
                        var text = (Text)eo.Clone();
                        text.Position = transform(text.Position);
                        return text;
                    }

                case EntityType.MText:
                    {
                        var mtext = (MText)eo.Clone();
                        mtext.Position = transform(mtext.Position);
                        return mtext;
                    }

                case EntityType.Circle:
                    {
                        var circle = (Circle)eo.Clone();
                        {
                            var c = transform(circle.Center);
                            if (origin != null) c -= origin;
                            circle.Center = c;
                        }
                        {
                            var r = transform(new Vector3D(circle.Radius, 0));
                            if (origin != null) r -= origin;
                            circle.Radius = r.Length;
                        }
                        return circle;
                    }

                case EntityType.Point:
                    {
                        var point = (Point)eo.Clone();
                        point.Position = transform(point.Position);
                        return point;
                    }

                case EntityType.LwPolyline:
                    {
                        var lw = (LwPolyline)eo.Clone();
                        lw.Vertexes.ForEach(w =>
                        {
                            w.Position = transform(w.Position).ToDxfVector2();
                        });
                        return lw;
                    }

                default: throw new NotImplementedException($"not implemented coord transform for entity [{eo.Type}]");
            }
        }

        /// <summary>
        /// add entity to the given dxf object ( it can be Dxfdocument or Block )
        /// optionally set layer
        /// </summary>        
        public static EntityObject AddEntity(this DxfObject dxfObj, EntityObject eo, Layer? layer = null)
        {
            if (dxfObj is DxfDocument dxfDoc) dxfDoc.AddEntity(eo);
            else if (dxfObj is Block dxfBlock) dxfBlock.Entities.Add(eo);
            else if (dxfObj is netDxf.Objects.Group dxfGroup) dxfGroup.Entities.Add(eo);
            else throw new ArgumentException($"dxfObj must DxfDocument or Block");

            if (layer != null) eo.Layer = layer;

            return eo;
        }

        /// <summary>
        /// add entity to the given dxf object ( it can be Dxfdocument or Block )
        /// optionally set layer
        /// </summary>        
        public static void AddEntities(this DxfObject dxfObj, IEnumerable<EntityObject> ents, Layer? layer = null)
        {
            foreach (var ent in ents) dxfObj.AddEntity(ent, layer);
        }

        /// <summary>
        /// Set layer of given set of dxf entities
        /// </summary>        
        public static IEnumerable<EntityObject> SetLayer(this IEnumerable<EntityObject> ents, Layer layer)
        {
            foreach (var x in ents) x.Layer = layer;
            return ents;
        }

        /// <summary>
        /// Creates and add dxf entities for a 3 axis of given length centered in given center point.
        /// </summary>        
        public static IEnumerable<EntityObject> DrawStar(this DxfObject dxfObj, Vector3D center, double L, Layer? layer = null)
        {
            var q = Star(center, L).ToList();

            foreach (var line in q) dxfObj.AddEntity(line, layer);

            return q;
        }

        /// <summary>
        /// Creates and add dxf entities for a 6 faces of a cube
        /// </summary>        
        public static IEnumerable<EntityObject> DrawCube(this DxfObject dxfObj, Vector3D center, double L, Layer? layer = null)
        {
            var ents = Cuboid(center, new Vector3D(L, L, L)).ToList();

            dxfObj.AddEntities(ents, layer);

            return ents;
        }

        /// <summary>
        /// Creates and add dxf entities for 6 faces of a cuboid
        /// </summary>        
        public static IEnumerable<EntityObject> DrawCuboid(this DxfObject dxfObj, Vector3D center, Vector3D size, Layer? layer = null)
        {
            var ents = Cuboid(center, size).ToList();

            dxfObj.AddEntities(ents, layer);

            return ents;
        }

        public static string CadScript(this Face3d face)
        {
            var sb = new StringBuilder();

            sb.Append(string.Format(CultureInfo.InvariantCulture, "_FACE {0},{1},{2} {3},{4},{5} {6},{7},{8}",
                face.FirstVertex.X, face.FirstVertex.Y, face.FirstVertex.Z,
                face.SecondVertex.X, face.SecondVertex.Y, face.SecondVertex.Z,
                face.ThirdVertex.X, face.ThirdVertex.Y, face.ThirdVertex.Z));

            if ((Vector3?)face.FourthVertex != null) sb.Append(string.Format(CultureInfo.InvariantCulture, " {0},{1},{2}",
                face.FourthVertex.X, face.FourthVertex.Y, face.FourthVertex.Z));

            sb.AppendLine();

            return PostProcessCadScript(sb.ToString());
        }

        /// <summary>
        /// tries to zoom dxf viewport on the given bbox
        /// </summary>        
        public static void AutoZoom(this DxfDocument dxf, BBox3D bbox)
        {
            var bbox_center = (bbox.Min + bbox.Max) / 2;
            var bbox_size = bbox.Max - bbox.Min;
            dxf.Viewport.ViewCenter = new Vector2(bbox.Min.X, bbox_center.Y);
            dxf.Viewport.ViewAspectRatio = bbox_size.X / (bbox_size.Y * 2);
        }

        public static EntityObject SetColor(this EntityObject eo, AciColor color)
        {
            eo.Color = color;
            return eo;
        }

        /*     /// <summary>
             /// retrieve rainbow rgb color from a double value between [0,1]
             /// </summary>        
             public static IRgb FromRainbow(this double factor)
             {*/
        /*
         * test : http://serennu.com/colour/hsltorgb.php
         * RGB: 218 165 32
         * HSL: 43° 74% 49%
         * Hex: #DAA520
        */

        /*
         * RAINBOW COLORS
         * 
         * red hsl(0,100%,50%)
         * orange hsl(30,100%,50%)
         * yellow hsl(60,100%,50%)
         * green hsl(120,100%,50%)
         * cyan hsl(180,100%,50%)
         * blue hsl(240,100%,50%)
         * purple hsl(270,100%,50%)
         * magenta hsl(300,100%,50%)
         */
        /*
       var hsl = new Hsl();

       // rainbow hue on range [0,240]
       hsl.H = (1.0 - factor) * 240;

       // 100% saturation
       hsl.S = 100;

       // 50% luminance
       hsl.L = 50;

       return hsl.ToRgb();
    }        */

        /*
                /// <summary>
                /// rgb contains r,g,b field each with 0-255 range integer value
                /// </summary>        
                public static AciColor AciColor(this IRgb rgb)
                {
                    var r = (int)rgb.R;
                    var g = (int)rgb.G;
                    var b = (int)rgb.B;

                    return netDxf.AciColor.FromTrueColor((r << 16) + (g << 8) + b);
                }*/

        public static IEnumerable<EntityObject> DrawTimeline(this DxfObject dxf, List<(DateTime from, DateTime to)> timeline,
        double textHeight = 2, double circleRadius = 1.5, double maxWidth = 180, double stopDays = 60, Func<DateTime, string> dtStr = null)
        {
            var q = timeline.OrderBy(w => w.from).ToList();

            var days = (timeline.Max(w => w.to) - timeline.Min(w => w.from)).TotalDays;

            var dayWidth = maxWidth / days;

            Func<double, double> dayGetX = (day) =>
            {
                return day * dayWidth;
            };

            if (dtStr == null) dtStr = (dt) => dt.Year.ToString();

            for (int i = 0; i < q.Count; ++i)
            {
                var prevWasStop = i == 0 || (q[i].from - q[i - 1].to).TotalDays > stopDays;

                var dayFromX = dayGetX((q[i].from - q[0].from).TotalDays);
                var dayToX = dayGetX((q[i].to - q[0].from).TotalDays);

                if (prevWasStop)
                {
                    // circle start
                    var circle = new Circle(new Vector3D(dayFromX, 0, 0), circleRadius);
                    yield return circle;

                    // prev vertical stop
                    if (i > 0)
                    {
                        var prevDayToX = dayGetX((q[i - 1].to - q[0].from).TotalDays);
                        yield return new Line(new Vector3D(prevDayToX, circleRadius, 0), new Vector3D(prevDayToX, -circleRadius, 0));
                    }
                }
                else
                {
                    // vertical start
                    yield return new Line(new Vector3D(dayFromX, circleRadius, 0), new Vector3D(dayFromX, -circleRadius, 0));
                }

                // horizontal line
                yield return new Line(new Vector3D(dayFromX + (prevWasStop ? circleRadius : 0), 0, 0), new Vector3D(dayToX, 0, 0));

                // year text
                {
                    var yearFrom = new DateTime(q[i].from.Year, 1, 1);
                    var yearFromX = dayGetX((yearFrom - q[0].from).TotalDays);
                    var txt = new Text(dtStr(yearFrom), new Vector3D(yearFromX, circleRadius + textHeight, 0), textHeight);
                    txt.Alignment = TextAlignment.BottomCenter;
                    yield return txt;
                }

                if (i == q.Count - 1)
                {
                    var yearTo = new DateTime(q[i].to.Year, 1, 1);
                    var yearToX = dayGetX((yearTo - q[0].from).TotalDays);
                    var txt = new Text(dtStr(yearTo), new Vector3D(yearToX, circleRadius + textHeight, 0), textHeight);
                    txt.Alignment = TextAlignment.BottomCenter;
                    yield return txt;
                }
            }

        }

    }

}
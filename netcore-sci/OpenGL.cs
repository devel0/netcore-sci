using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using QuantumConcepts.Formats.StereoLithography;
using System.Linq;

namespace SearchAThing
{

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GLVertexWithNormal
    {
        public Vector3 Position;
        public Vector3 Normal;
    }

    public class GLVertexWithNormalNfo
    {
        public Vector3D Position;
        public Vector3D Normal;
    }

    public class GLVertexManager
    {

        public double Tol { get; set; }

        /// <summary>
        /// list of points
        /// entry i-th is coordinate of indexes i-th
        /// </summary>
        List<GLVertexWithNormalNfo> vtxs = new List<GLVertexWithNormalNfo>();

        public IReadOnlyList<GLVertexWithNormalNfo> Vtxs => vtxs;

        /// <summary>
        /// dictionary key:vertex.ToString() value:vertex index
        /// </summary>
        Dictionary<string, uint> idxs = new Dictionary<string, uint>();

        public IReadOnlyDictionary<string, uint> Idxs => idxs;

        /// <summary>
        /// list of point indexes foreach named figure
        /// </summary>
        Dictionary<string, List<uint>> figureIdxs = new Dictionary<string, List<uint>>();

        HashSet<string> triangleFigures = new HashSet<string>();

        public BBox3D BBox { get; private set; } = new BBox3D();

        public GLVertexManager(double tol)
        {
            Tol = tol;
        }

        /// <summary>
        /// retrieve idx of given point and update bbox.
        /// </summary>
        /// <param name="v">point coordinate</param>
        /// <returns>index of given point</returns>        
        public uint AddPoint(Vector3D v)
        {
            uint res = 0;
            var str = v.ToString(Tol);
            if (!idxs.TryGetValue(str, out res))
            {
                res = (uint)vtxs.Count;
                vtxs.Add(new GLVertexWithNormalNfo { Position = v });
                idxs.Add(str, res);

                BBox.ApplyUnion(v);
            }
            return res;
        }

        public IReadOnlyList<uint> AddFigures(string figureName, params Vector3D[][] figuresPoints) =>
            AddFigures(figureName, (IEnumerable<Vector3D[]>)figuresPoints);

        public IReadOnlyList<uint> AddFigures(string figureName, IEnumerable<Vector3D[]> figuresPoints)
        {
            List<uint> idxs = null;
            if (!figureIdxs.TryGetValue(figureName, out idxs))
            {
                idxs = new List<uint>();
                figureIdxs.Add(figureName, idxs);
            }

            foreach (var figurePoints in figuresPoints)
            {
                foreach (var pt in figurePoints) idxs.Add(AddPoint(pt));
            }

            return idxs;
        }

        public void AddTriangles(string figureName, params Vector3D[][] triangles)
        {
            AddTriangles(figureName, (IEnumerable<Vector3D[]>)triangles);
        }

        public void AddTriangles(string figureName, IEnumerable<Vector3D[]> triangles)
        {
            triangleFigures.Add(figureName);

            List<uint> idxs = null;
            if (!figureIdxs.TryGetValue(figureName, out idxs))
            {
                idxs = new List<uint>();
                figureIdxs.Add(figureName, idxs);
            }

            foreach (var tri in triangles)
            {
                var aIdx = AddPoint(tri[0]);
                var bIdx = AddPoint(tri[1]);
                var cIdx = AddPoint(tri[2]);

                idxs.AddRange(new[] { aIdx, bIdx, cIdx });
            }
        }

        /// <summary>
        /// adds given STL facets
        /// </summary>
        /// <param name="figureName">name of figure for idxs retrieval</param>
        /// <param name="facets">STL facets (see STLDocument.Read(stream).Facets)</param>
        public void AddFaces(string figureName, IEnumerable<Facet> facets)
        {
            AddTriangles(figureName, facets.Select(w => w.Vertices.Select(w => (Vector3D)w).ToArray()));
        }

        public GLVertexWithNormal[] BuildPoints()
        {
            for (int i = 0; i < vtxs.Count; ++i)
            {
                var v = vtxs[i];
                v.Normal = new Vector3();
            }

            foreach (var triFigure in triangleFigures)
            {
                var idxs = figureIdxs[triFigure];

                for (int i = 0; i < idxs.Count; i += 3)
                {
                    var a = vtxs[(int)idxs[i]];
                    var b = vtxs[(int)idxs[i + 1]];
                    var c = vtxs[(int)idxs[i + 2]];

                    var norm = ((c.Position - b.Position).CrossProduct(a.Position - b.Position)).Normalized();

                    a.Normal += norm;
                    b.Normal += norm;
                    c.Normal += norm;
                }
            }

            for (int i = 0; i < vtxs.Count; i++)
            {
                var v = vtxs[i];
                v.Normal = Vector3.Normalize(vtxs[i].Normal);
            }

            return vtxs.Select(w => new GLVertexWithNormal { Position = w.Position, Normal = w.Normal }).ToArray();
        }

        public uint[] BuildIdxs(string figureName) => figureIdxs[figureName].ToArray();

    }

    public static partial class SciExt
    {

        public static GLVertexWithNormal ToGLTriangleVertex(this Vector3D v)
        {
            return new GLVertexWithNormal
            {
                Position = new Vector3((float)v.X, (float)v.Y, (float)v.Z)
            };
        }

    }

}
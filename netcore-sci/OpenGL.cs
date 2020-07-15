using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using QuantumConcepts.Formats.StereoLithography;
using System.Linq;

namespace SearchAThing
{

    /// <summary>
    /// contains a vector3
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GLVertex
    {
        public Vector3 Position;        
    }

    /// <summary>
    /// structure used by GLVertexManager when BuildPoints called
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GLVertexWithNormal
    {
        public Vector3 Position;
        public Vector3 Normal;
    }

    /// <summary>
    /// used by GLVertexManager to store info about position, normal of vertexes
    /// </summary>
    public class GLVertexWithNormalNfo
    {
        public Vector3D Position;
        public Vector3D Normal;
    }

    /// <summary>
    /// Helper to manage gl vertexes for figures such as triangles, lines
    /// </summary>
    public class GLVertexManager
    {

        /// <summary>
        /// tolerance used for dictionarized vertex storage
        /// </summary>        
        public double Tol { get; set; }

        /// <summary>
        /// list of points
        /// entry i-th is coordinate of indexes i-th
        /// </summary>
        List<GLVertexWithNormalNfo> vtxs = new List<GLVertexWithNormalNfo>();

        /// <summary>
        /// readonly list of all inserted vertexes with normals ( normals updated only after BuildPoints() call )
        /// </summary>
        public IReadOnlyList<GLVertexWithNormalNfo> Vtxs => vtxs;

        /// <summary>
        /// dictionary key:vertex.ToString() value:vertex index
        /// </summary>
        Dictionary<string, uint> idxs = new Dictionary<string, uint>();

        /// <summary>
        /// readonly list of all indexes subdivided for figureNames
        /// </summary>
        public IReadOnlyDictionary<string, uint> Idxs => idxs;

        /// <summary>
        /// list of point indexes foreach named figure
        /// </summary>
        Dictionary<string, List<uint>> figureIdxs = new Dictionary<string, List<uint>>();

        /// <summary>
        /// list of figure names belonging to AddTriangles methods
        /// </summary>
        HashSet<string> triangleFigures = new HashSet<string>();

        /// <summary>
        /// bbox of all inserted points, updated after each Add methods
        /// </summary>
        public BBox3D BBox { get; private set; } = new BBox3D();

        /// <summary>
        /// construct a vertex manager that stores given points with given tolerance for indexing
        /// </summary>
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

        /// <summary>
        /// add given figures points array to the given figureName set
        /// </summary>
        /// <param name="figureName">set name for the figure points</param>
        /// <param name="figuresPoints">array of points array that represents a set of figures ( 3 pts for triangles, 2 pts for lines )</param>
        /// <returns>readonly list of figure indexes</returns>
        public IReadOnlyList<uint> AddFigures(string figureName, params Vector3D[][] figuresPoints) =>
            AddFigures(figureName, (IEnumerable<Vector3D[]>)figuresPoints);

        /// <summary>
        /// add given figures points array to the given figureName set
        /// </summary>
        /// <param name="figureName">set name for the figure points</param>
        /// <param name="figuresPoints">array of points array that represents a set of figures ( 3 pts for triangles, 2 pts for lines )</param>
        /// <returns>readonly list of figure indexes</returns>
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

        /// <summary>
        /// add given triangles array to the given figureName set
        /// </summary>
        /// <param name="figureName">set name for the triangles</param>
        /// <param name="triangles">array of triangles ( 3 pts each )</param>
        /// <returns>readonly list of triangles indexes</returns>
        public IReadOnlyList<uint> AddTriangles(string figureName, params Vector3D[][] triangles) =>
            AddTriangles(figureName, (IEnumerable<Vector3D[]>)triangles);

        /// <summary>
        /// add given triangles array to the given figureName set
        /// </summary>
        /// <param name="figureName">set name for the triangles</param>
        /// <param name="triangles">array of triangles ( 3 pts each )</param>
        /// <returns>readonly list of triangles indexes</returns>
        public IReadOnlyList<uint> AddTriangles(string figureName, IEnumerable<Vector3D[]> triangles)
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

            return idxs;
        }

        /// <summary>
        /// add given STL facets to the given figureName set
        /// </summary>
        /// <param name="figureName">set name for the triangles</param>
        /// <param name="facets">STL facets (see STLDocument.Read(stream).Facets)</param>
        /// <returns>readonly list of triangles indexes</returns>
        public void AddFaces(string figureName, IEnumerable<Facet> facets) =>
            AddTriangles(figureName, facets.Select(w => w.Vertices.Select(w => (Vector3D)w).ToArray()));

        /// <summary>
        /// to be called after all figures inserted; this will rebuild vertex normals
        /// </summary>
        /// <returns>array of vertex with normal suitable to use with GL array</returns>
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

        /// <summary>
        /// retrieve the set of indexes belonging to given figure name
        /// </summary>
        /// <param name="figureName">figure name for which to retrieve indexes</param>
        /// <returns>indexes belonging to given figure name</returns>
        public uint[] BuildIdxs(string figureName) => figureIdxs[figureName].ToArray();

    }

    public static partial class SciExt
    {

        /// <summary>
        /// create GLVertexWithNormal from given vector ( normal = Vector.Zero )
        /// </summary>
        /// <param name="v">vector from which build GLVertexWithNormal</param>
        /// <returns>GLVertexWithNormal constructed from given vector ( normal = Vector.Zero )</returns>
        public static GLVertexWithNormal ToGLTriangleVertex(this Vector3D v)
        {
            return new GLVertexWithNormal
            {
                Position = new Vector3((float)v.X, (float)v.Y, (float)v.Z),
                Normal = Vector3D.Zero
            };
        }

    }

}
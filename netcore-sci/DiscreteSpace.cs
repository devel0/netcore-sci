using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;

namespace SearchAThing
{

    /// <summary>
    /// base item for DiscreteSpace
    /// </summary>    
    public class DiscreteSpaceItem<T>
    {

        public Vector3D Mean { get; private set; }

        public T? Item { get; private set; }

        internal DiscreteSpaceItem(Vector3D pt)
        {
            Mean = pt;
        }

        public DiscreteSpaceItem(T _Item, Func<T, Vector3D> entPoint)
        {
            Item = _Item;
            Mean = entPoint(Item);
        }

        public DiscreteSpaceItem(T _Item, Func<T, IEnumerable<Vector3D>> entPoints)
        {
            Item = _Item;
            Mean = entPoints(Item).Mean();
        }

    }

    /// <summary>
    /// comparer to search in DiscreteSpace
    /// </summary>    
    public class DiscreteSpaceItemComparer<T> : IComparer<DiscreteSpaceItem<T>>
    {
        double tol;
        int ord;

        public DiscreteSpaceItemComparer(double _tol, int _ord)
        {
            tol = _tol;
            ord = _ord;
        }

        public int Compare(DiscreteSpaceItem<T>? x, DiscreteSpaceItem<T>? y)
        {
            if (x == null || y == null) return -1;

            return x.Mean.GetOrd(ord).CompareTol(tol, y.Mean.GetOrd(ord));
        }
    }

    /// <summary>
    /// organize given item list into a discretized space to allow fast query of elements in a space region
    /// </summary>
    public class DiscreteSpace<T>
    {

        List<DiscreteSpaceItem<T>>[] sorted;
        DiscreteSpaceItemComparer<T>[] cmp;

        public IEnumerable<T> Items => sorted.SelectMany(w => w.Select(w => w.Item))
            .Where(r => r != null).Select(w => w!);

        double tol;
        int spaceDim;

        DiscreteSpace(double _tol, List<DiscreteSpaceItem<T>> q, int _spaceDim)
        {
            tol = _tol;
            spaceDim = _spaceDim;

            sorted = new List<DiscreteSpaceItem<T>>[spaceDim];
            cmp = new DiscreteSpaceItemComparer<T>[spaceDim];

            for (int ord = 0; ord < spaceDim; ++ord)
            {
                sorted[ord] = q.OrderBy(w => w.Mean.GetOrd(ord)).ToList();
                cmp[ord] = new DiscreteSpaceItemComparer<T>(tol, ord);
            }
        }

        Func<T, Vector3D>? entGetPoint;

        /// <summary>
        /// Build discrete space
        /// </summary>
        /// <param name="_tol">length tolerance</param>
        /// <param name="ents">list of entities to discretize</param>
        /// <param name="entPoints">function that retrieve relevant points from templated item</param>
        /// <param name="_spaceDim">search space dimension (2=2D 3=3D)</param>        
        /// <returns>discrete space object</returns>     
        public DiscreteSpace(double _tol, IEnumerable<T> ents, Func<T, IEnumerable<Vector3D>> entPoints, int _spaceDim) :
            this(_tol, ents.Select(ent => new DiscreteSpaceItem<T>(ent, entPoints)).ToList(), _spaceDim)
        {
            entGetPoint = (t) => entPoints(t).Mean();
        }

        /// <summary>
        /// Build discrete space
        /// </summary>
        /// <param name="_tol">length tolerance</param>
        /// <param name="ents">list of entities to discretize</param>
        /// <param name="entPoint">function that retrieve relevant point from templated item</param>
        /// <param name="_spaceDim">search space dimension (2=2D 3=3D)</param>        
        /// <returns>discrete space object</returns>     
        public DiscreteSpace(double _tol, IEnumerable<T> ents, Func<T, Vector3D> entPoint, int _spaceDim) :
            this(_tol, ents.Select(ent => new DiscreteSpaceItem<T>(ent, entPoint)).ToList(), _spaceDim)
        {
            entGetPoint = (t) => entPoint(t);
        }

        IEnumerable<T> GetItemsAt_R(int ord, double off, double maxDist)
        {
            var itosearch = new DiscreteSpaceItem<T>(Vector3D.Axis(ord) * off);

            // bin search return 0,(n-1) if found or negative number so that ~result is the index of 
            // the first element greather than those we search for
            var bsr = sorted[ord].BinarySearch(itosearch, cmp[ord]);

            int idx = 0;
            if (bsr < 0)
                idx = ~bsr;
            else
                idx = bsr;

            DiscreteSpaceItem<T>? dsi = null;

            // right search
            for (int i = idx; i < sorted[ord].Count && Abs((dsi = sorted[ord][i]).Mean.GetOrd(ord) - off) <= maxDist; ++i)
            {
                if (dsi.Item != null)
                    yield return dsi.Item;
            }

            // left search
            for (int i = idx - 1; i >= 0 && Abs((dsi = sorted[ord][i]).Mean.GetOrd(ord) - off) <= maxDist; --i)
            {
                if (dsi.Item != null)
                    yield return dsi.Item;
            }

        }

        IEnumerable<T> GetItemsInRange(int ord, double offMin, double offMax)
        {
            var itosearch = new DiscreteSpaceItem<T>(Vector3D.Axis(ord) * offMin);

            // bin search return 0,(n-1) if found or negative number so that ~result is the index of 
            // the first element greather than those we search for
            var bsr = sorted[ord].BinarySearch(itosearch, cmp[ord]);

            int idx = 0;
            if (bsr < 0)
                idx = ~bsr;
            else
                idx = bsr;

            DiscreteSpaceItem<T>? dsi = null;

            // right search
            for (int i = idx; i < sorted[ord].Count && (dsi = sorted[ord][i]).Mean.GetOrd(ord) <= offMax; ++i)
            {
                if (dsi.Item != null)
                    yield return dsi.Item;
            }
        }

        enum SearchDir { none, reduce, increase };

        public IEnumerable<T> Search(Vector3D from, Vector3D to, int maxCnt = 10)
        {
            /*

                     -------------------------------------d------------------------------------>
                     from.....................................................................to
                     0/4               1/4               2/4               3/4               4/4                     
            q1       ================== a =================
            q2                                            ================= b ==================

            */

            var d = to - from;
            var dLen = d.Length;

            var a = from + d * .25; // 1/4
            var q1 = GetItemsAt(a, dLen * .25); // 1/4
            if (q1.Count() <= maxCnt)
            {
                foreach (var x in q1) yield return x;
            }
            else if (q1.Any())
            {
                foreach (var x in Search(from, from + d / 2)) yield return x;
            }
            else
            {
                var b = from + d * .75; // 3/4
                var q2 = GetItemsAt(b, dLen * .25); // 1/4
                if (q2.Count() <= maxCnt)
                {
                    foreach (var x in q2) yield return x;
                }
                else if (q2.Any())
                {
                    foreach (var x in Search(from + d / 2, to)) yield return x;
                }
            }
        }

        public IEnumerable<T> GetItemsAtBySubdiv(Vector3D pt, BBox3D bbox, int maxRes = 1, int subDivs = 100)
            => GetItemsAtBySubdiv(pt, bbox, maxRes, subDivs, SearchDir.none);

        IEnumerable<T> GetItemsAtBySubdiv(Vector3D pt, BBox3D bbox, int maxRes, int subDivs, SearchDir dir)
        {
            if (!bbox.Contains(tol, pt))
                yield break;

            var bboxSize = bbox.Size;
            var maxDist = bboxSize.Length / subDivs;

            var q = GetItemsAt(pt, maxDist);
            var qcnt = q.Count();

            if (qcnt == 0) // found none
            {
                if (dir == SearchDir.reduce) yield break;

                if (subDivs > 2)
                {
                    subDivs /= 2; // recurse with bigger region search
                    foreach (var x in GetItemsAtBySubdiv(pt, bbox, maxRes, subDivs, SearchDir.increase))
                    {
                        yield return x;
                    }
                }
                else
                    yield break;
            }
            else // found some
            {
                if (qcnt <= maxRes) // found enough
                {
                    foreach (var x in q)
                    {
                        yield return x;
                    }
                }
                else // found too much
                {
                    subDivs *= 2; // recurse with smaller region search

                    var qs = GetItemsAtBySubdiv(pt, bbox, maxRes, subDivs, SearchDir.reduce);

                    if (!qs.Any()) // cannot recurse with smaller because will get none
                    {
                        if (entGetPoint == null) throw new Exception($"entGetPoint undefined");

                        foreach (var x in q.OrderBy(u => (entGetPoint(u) - pt).Length).Take(maxRes))
                        {
                            yield return x;
                        }
                    }
                    else
                    {
                        foreach (var x in qs)
                        {
                            yield return x;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// retrieve items that resides in the space at given point with given extents max distance
        /// </summary>
        /// <param name="pt">point to query elements near</param>
        /// <param name="maxDist">distance from the given pt to include queried items</param>
        /// <returns>list of items belonging to sphere centered at pt given radius equals to given maxDist</returns>
        public IEnumerable<T> GetItemsAt(Vector3D pt, double maxDist)
        {
            IEnumerable<T>? set = null;

            if (spaceDim <= 0) throw new Exception($"invalid spacedim {spaceDim}");

            for (int dim = 0; dim < spaceDim; ++dim)
            {
                var thisDimSet = GetItemsAt_R(dim, pt.GetOrd(dim), maxDist);

                if (dim == 0)
                    set = thisDimSet;
                else
                    set = set!.Intersect(thisDimSet);
            }

            return set!;
        }

        public IEnumerable<T> GetItemsInBBox(BBox3D bbox)
        {
            IEnumerable<T>? set = null;

            if (spaceDim <= 0) throw new Exception($"invalid spacedim {spaceDim}");

            for (int dim = 0; dim < spaceDim; ++dim)
            {
                var thisDimSet = GetItemsInRange(dim, bbox.Min.GetOrd(dim), bbox.Max.GetOrd(dim));

                if (dim == 0)
                    set = thisDimSet;
                else
                    set = set!.Intersect(thisDimSet);
            }

            return set!;
        }

    }

}

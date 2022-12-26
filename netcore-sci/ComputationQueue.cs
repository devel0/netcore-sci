namespace SearchAThing
{

	/// <summary>
	/// helper circular queue manager for computation.
	/// at constructor the maxSize ( default=3 ) can be specified to allow retrieval of previous computed items
	/// </summary>
	/// <typeparam name="T">type that hold computation data</typeparam>
	public class ComputationQueue<T> where T : class
    {
        List<T> lst = new List<T>();

        /// <summary>
        /// max queue size
        /// </summary>        
        public int MaxSize { get; private set; }

        int CurrentIndex = -1;

        /// <summary>
        /// actual inserted items in the queue
        /// </summary>
        public int Count => lst.Count;

        /// <summary>
        /// retrieve item from the queue
        /// </summary>
        /// <param name="prevCnt">0 for last inserted, 1 for prev, 2 for prevPrev, ...</param>
        /// <returns>prev-th item in the queue</returns>
        public T? GetItem(int prevCnt)
        {
            if (prevCnt < 0) throw new ArgumentException("specify positive argument to locate previous items in the queue");

            if (prevCnt >= Count) return null;

            var off = CurrentIndex - prevCnt;
            if (off < 0) off += Count;
            return lst[off];
        }

        /// <summary>
        /// last inserted item
        /// </summary>        
        public T? Current => GetItem(0);

        /// <summary>
        /// previous inserted item ( penultimate )
        /// </summary>        
        public T? Prev => GetItem(1);

        /// <summary>
        /// previous previous inserted item ( antepeunltimate )
        /// </summary>    
        public T? PrevPrev => GetItem(2);

        /// <summary>
        /// construct a computation queue with given size.
        /// if queue size already at maxSize older element will be overwritten.
        /// </summary>
        /// <param name="maxSize">max circular queue size</param>
        public ComputationQueue(int maxSize = 3)
        {
            MaxSize = maxSize;
        }

        /// <summary>
        /// insert element in the queue.
        /// if queue size already at maxSize older element will be overwritten
        /// </summary>
        /// <param name="d">data to enqueue</param>
        public void Enq(T d)
        {
            if (lst.Count < MaxSize)
            {
                lst.Add(d);
                ++CurrentIndex;
            }
            else
            {
                ++CurrentIndex;
                if (CurrentIndex >= MaxSize) CurrentIndex = 0;
                lst[CurrentIndex] = d;
            }
        }

    }

}
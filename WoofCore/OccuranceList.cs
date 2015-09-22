using System.Collections.Generic;

namespace Woof.Core {
    
    /// <summary>
    /// Element occurance list
    /// </summary>
    public class OccuranceList<T> {
        /// <summary>
        /// Capacity of the list if not defined explicitly
        /// </summary>
        private const int DefaultCapacity = 255;
        /// <summary>
        /// Internal linekd list
        /// </summary>
        private LinkedList<T> Items;
        /// <summary>
        /// List capacity
        /// </summary>
        private int Capacity = DefaultCapacity;

        /// <summary>
        /// Creates new occurance list with default capacity
        /// </summary>
        public OccuranceList() { Items = new LinkedList<T>(); }

        /// <summary>
        /// Creates new occurance list with defined capacity
        /// </summary>
        /// <param name="capacity"></param>
        public OccuranceList(int capacity) { Items = new LinkedList<T>(); Capacity = capacity; }

        /// <summary>
        /// Adds specified item as last, remove first if capatity exceeded
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item) {
            if (Items.Count < Capacity) Items.AddLast(item);
            else { Items.RemoveFirst(); Items.AddLast(item); }
        }

        /// <summary>
        /// Removes all occurances of specified item from list
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item) { while (Items.Remove(item)) ; }

        /// <summary>
        /// Returns all occurances of the specified item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int Count(T item) {
            var count = 0;
            foreach (T i in Items) if (i.Equals(item)) count++;
            return count;
        }

        /// <summary>
        /// Checks if the specified item occurs the specified number of times
        /// </summary>
        /// <param name="item"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public bool MeetsLimit(T item, int limit) {
            if (Count(item) + 2 > limit) { Remove(item); return true; } else { Add(item); return false; }
        }

    }

}
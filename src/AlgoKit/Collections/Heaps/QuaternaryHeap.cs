﻿using System.Collections.Generic;

namespace AlgoKit.Collections.Heaps
{
    /// <summary>
    /// Represents an implicit heap-ordered complete quaternary tree.
    /// </summary>
    public class QuaternaryHeap<T> : ArrayHeap<T>
    {
        /// <summary>
        /// Creates an empty quaternary heap.
        /// </summary>
        /// <param name="comparer">The comparer used to determine whether one object should be extracted from the heap earlier than the other one.</param>
        public QuaternaryHeap(IComparer<T> comparer) : base(4, comparer)
        {
        }

        /// <summary>
        /// Creates a quaternary heap out of given list of elements in linear time.
        /// </summary>
        /// <param name="items">The list of items to build a heap from.</param>
        /// <param name="comparer">The comparer used to determine whether one object should be extracted from the heap earlier than the other one.</param>
        public QuaternaryHeap(List<T> items, IComparer<T> comparer) : base(4, items, comparer)
        {
        }
    }
}
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AlgoKit.Collections.Heaps
{
    /// <summary>
    /// Represents a heap-ordered complete d-ary tree.
    /// </summary>
    public class DAryHeap<T> : ICollection<T>
    {
        private readonly List<T> elements;
        private readonly IComparer<T> comparer;

        /// <summary>
        /// Creates an empty d-ary heap.
        /// </summary>
        /// <param name="arity">The arity of the heap.</param>
        /// <param name="comparer">The comparer used to determine whether one object should be extracted from the heap earlier than the other one.</param>
        public DAryHeap(int arity, IComparer<T> comparer) : this(arity, new List<T>(), comparer)
        {
        }

        /// <summary>
        /// Creates a d-ary heap out of given list of elements in linear time.
        /// </summary>
        /// <param name="arity">The arity of the heap.</param>
        /// <param name="items">The list of items to build a heap from.</param>
        /// <param name="comparer">The comparer used to determine whether one object should be extracted from the heap earlier than the other one.</param>
        public DAryHeap(int arity, List<T> items, IComparer<T> comparer)
        {
            if (arity < 1)
                throw new ArgumentOutOfRangeException($"Expected arity to be at least 1, but was {arity}.");

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            this.Arity = arity;
            this.elements = items;
            this.comparer = comparer;

            if (this.Count > 1)
                this.Heapify();
        }

        /// <summary>
        /// Gets the arity of the heap.
        /// </summary>
        public int Arity { get; }

        /// <summary>
        /// Gets the number of elements contained in the heap.
        /// </summary>
        public int Count => this.elements.Count;

        /// <summary>
        /// Gets a value indicating whether the heap is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Returns true if the heap is empty, false otherwise.
        /// </summary>
        public bool IsEmpty => this.elements.Count == 0;

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        public T this[int index]
        {
            get { return this.elements.ElementAt(index); }
            set { this.Replace(index, value); }
        }

        /// <summary>
        /// Gets the top element of the heap.
        /// </summary>
        public T Top()
        {
            if (this.IsEmpty)
                throw new InvalidOperationException("The heap is empty.");

            return this.elements[0];

        }

        /// <summary>
        /// Returns the top element after removing it from the heap.
        /// </summary>
        public T Pop()
        {
            var result = this.Top();
            this.RemoveTop();
            return result;
        }

        /// <summary>
        /// Removes the top element from the heap. 
        /// </summary>
        public void RemoveTop()
        {
            if (this.IsEmpty)
                throw new InvalidOperationException("The heap is empty.");

            this.RemoveAt(0);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index
        /// of the first occurrence within the entire heap.
        /// </summary>
        public int IndexOf(T element) => this.elements.IndexOf(element);

        /// <summary>
        /// Determines whether an element is in the heap.
        /// </summary>
        public bool Contains(T element) => this.elements.Contains(element);

        /// <summary>
        /// Copies the entire heap to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from this heap.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.elements.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes all elements from the heap.
        /// </summary>
        public void Clear()
        {
            this.elements.Clear();
        }

        /// <summary>
        /// Adds an object to the heap.
        /// </summary>
        public void Add(T item)
        {
            // Add node at the end
            var index = this.Count;
            this.elements.Add(item);

            if (index == 0)
                return;

            // Restore the heap order
            this.MoveUp(index);
        }

        /// <summary>
        /// Removes the first occurrence of a speficic object from the heap.
        /// </summary>
        /// <param name="item">The object to remove from the heap. The value can be null for reference types.</param>
        public bool Remove(T item)
        {
            var index = this.IndexOf(item);

            if (index == -1)
                return false;

            this.RemoveAt(index);
            return true;
        }
        
        /// <summary>
        /// Replaces the element at the specified index with a new object.
        /// </summary>
        /// <param name="index">The index of the node to be modified.</param>
        /// <param name="item">The new value of the node.</param>
        public void Replace(int index, T item)
        {
            var current = this.elements[index];
            this.elements[index] = item;

            if (this.WouldBeExtractedEarlier(item, current))
            {
                this.MoveUp(index);
            }
            else if (this.WouldBeExtractedEarlier(current, item))
            {
                this.MoveDown(index);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the heap.
        /// </summary>
        public IEnumerator<T> GetEnumerator() => this.elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Determines whether one object should be extracted from the heap earlier
        /// than the second one.
        /// </summary>
        private bool WouldBeExtractedEarlier(T first, T second)
        {
            return this.comparer.Compare(first, second) < 0;
        }

        /// <summary>
        /// Gets the index of an element's parent.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetParentIndex(int index) => (index - 1) / this.Arity;

        /// <summary>
        /// Enumerates the indexes of element's children.
        /// </summary>
        private IEnumerable<int> EnumerateChildrenIndexes(int index)
        {
            var leftmostChild = this.Arity * index + 1;
            for (var i = 0; i < this.Arity; ++i)
                yield return leftmostChild + i;
        }

        /// <summary>
        /// Removes the element at the specified index of the heap.
        /// </summary>
        /// <param name="index">The index of the node to be removed.</param>
        private void RemoveAt(int index)
        {
            // Replace the node at the specified index by the very last element
            var lastIndex = this.Count - 1;
            this.elements[index] = this.elements[lastIndex];
            this.elements.RemoveAt(lastIndex);

            // The heap property needs to be restored (in general) if there are any
            // remaining nodes after the removal.
            if (lastIndex > 0)
                this.MoveDown(index);
        }

        /// <summary>
        /// Converts an unordered list into a heap.
        /// </summary>
        private void Heapify()
        {
            // Leaves of the tree are in fact 1-element heaps, for which there
            // is no need to correct them. The heap property needs to be restored
            // only for higher nodes, starting from the first node that has children.
            // It is the parent of the very last element in the array.

            var lastElementIndex = this.elements.Count - 1;
            var lastParentWithChildren = this.GetParentIndex(lastElementIndex);

            for (var index = lastParentWithChildren; index >= 0; --index)
                this.MoveDown(index);
        }

        /// <summary>
        /// Moves the node up in the tree to restore heap order.
        /// </summary>
        /// <param name="index">The index of the node to be moved up.</param>
        private void MoveUp(int index)
        {
            var toMove = this.elements[index];

            // Instead of swapping items all th way to the root, we will perform
            // a similar optimization as in the insertion sort.

            while (index > 0)
            {
                var parentIndex = this.GetParentIndex(index);
                var parent = this.elements[parentIndex];

                if (this.WouldBeExtractedEarlier(toMove, parent))
                {
                    this.elements[index] = parent;
                    index = parentIndex;
                }
                else
                    break;
            }

            this.elements[index] = toMove;
        }

        /// <summary>
        /// Moves a node down in the tree to restore heap order.
        /// </summary>
        /// <param name="index">The index of the node to be moved down.</param>
        private void MoveDown(int index)
        {
            // The node to moved down will not actually be swapped every time.
            // Rather, values on the affected path will move up, leaving a free spot
            // for this value to drop in. Similar optimization as in the insertion sort.
            var toMove = this.elements[index];

            // Locally, it is the element that would be extracted first.
            var localPeekIndex = index;
            var localPeek = toMove;

            while (true)
            {
                // Check if the current node would really be extracted first, or maybe
                // one of its children would be extracted earlier.
                var childrenIndexes = this.EnumerateChildrenIndexes(index);

                foreach (var childIndex in childrenIndexes)
                {
                    // If this index is not valid, the rest will be invalid as well.
                    if (childIndex >= this.Count)
                        break;

                    var child = this.elements[childIndex];
                    if (this.WouldBeExtractedEarlier(child, localPeek))
                    {
                        localPeekIndex = childIndex;
                        localPeek = child;
                    }
                }
                
                // In case the current node would really be extracted first, there is
                // nothing more to do - a free spot for it was found.
                if (index == localPeekIndex)
                    break;

                // Move the top value up by one node and now investigate the
                // node that was considered to be the top child (recursive).
                this.elements[index] = localPeek;
                index = localPeekIndex;
                localPeek = toMove;
            }

            this.elements[index] = toMove;
        }
    }
}
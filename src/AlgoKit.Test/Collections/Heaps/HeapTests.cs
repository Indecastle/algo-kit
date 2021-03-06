using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AlgoKit.Collections.Heaps;
using AlgoKit.Extensions;
using MoreLinq;
using Xunit;

namespace AlgoKit.Test.Collections.Heaps
{
    public class _ArrayHeapTests : HeapTests<ArrayHeap<int, string>, ArrayHeapNode<int, string>>{}
    public class _BinomialHeapTests : HeapTests<BinomialHeap<int, string>, BinomialHeapNode<int, string>>{}
    public class _PairingHeapTests : HeapTests<PairingHeap<int, string>, PairingHeapNode<int, string>>{}

    public abstract class HeapTests<THeap, THandle>
        where THeap : BaseHeap<int, string, THandle, THeap>
        where THandle : class, IHeapNode<int, string>
    {
        private static THeap CreateHeapInstance()
        {

            return (THeap) Activator.CreateInstance(typeof(THeap), Comparer<int>.Default);
        }

        public class HeapConfiguration
        {
            public int HeapSize { get; }
            public int KeyLimit { get; }

            public HeapConfiguration(int heapSize, int keyLimit)
            {
                this.HeapSize = heapSize;
                this.KeyLimit = keyLimit;
            }

            public List<int> GenerateKeys(Random random, int? count = null)
            {
                return Enumerable.Range(1, count ?? this.HeapSize)
                    .Select(x => random.Next(this.KeyLimit))
                    .ToList();
            }

            public int CalculateIterations(int desiredNumber)
            {
                if (this.HeapSize <= 10)
                    return desiredNumber;

                var thousands = desiredNumber / 1000;
                var weight = thousands / Math.Log10(this.HeapSize);
                return Math.Min(1, (int) weight);
            }
        }

        public static IEnumerable<object[]> GetHeapConfigurations()
        {
            yield return new HeapConfiguration(1, 1).Yield<object>().ToArray();;

            for (var i = 1; i <= 10; ++i)
                yield return new HeapConfiguration(i, 2*i + 1).Yield<object>().ToArray();;

            yield return new HeapConfiguration(100, int.MaxValue).Yield<object>().ToArray();;
            yield return new HeapConfiguration(1000, int.MaxValue).Yield<object>().ToArray();;
        }

        private static int Peek(IEnumerable<HandleKeyPair> list) => list.Select(x => x.Key).Min();

        [Fact]
        public void Should_not_allow_creating_a_heap_with_null_comparer()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                try
                {
                    var heap = (THeap) Activator.CreateInstance(typeof(THeap), (IComparer<int>) null);
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            });
        }

        [Fact]
        public void Should_not_allow_passing_null_handle_or_heap_as_a_parameter()
        {
            // Arrange
            var heap = CreateHeapInstance();

            var testDelegates = new Action[]
            {
                () => heap.Remove((IHeapNode<int, string>)null),
                () => heap.Remove(null),

                () => heap.Merge((IHeap<int, string>)null),
                () => heap.Merge(null),

                () => heap.Update((IHeapNode<int, string>)null, 0),
                () => heap.Update(null, 0)
            };

            // Act & Assert
            foreach (var testDelegate in testDelegates)
                Assert.Throws<ArgumentNullException>(testDelegate);
        }

        [Theory]
        [MemberData(nameof(GetHeapConfigurations))]
        public void Top_should_be_properly_maintained_after_addition(HeapConfiguration conf)
        {
            var iterations = conf.CalculateIterations(15000);
            for (var seed = 0; seed < iterations; ++seed)
            {
                // Arrange
                var keys = conf.GenerateKeys(new Random(seed));
                var heap = CreateHeapInstance();
                var top = int.MaxValue;
                var count = 0;

                foreach (var key in keys)
                {
                    if (key < top)
                        top = key;

                    // Act
                    heap.Add(key, Guid.NewGuid().ToString());
                    ++count;

                    // Assert
                    Assert.Equal(top, heap.Peek().Key);
                    Assert.Equal(count, heap.Count);
                }
            }
        }

        [Fact]
        public void Should_not_allow_peek_or_pop_from_empty_heap()
        {
            // Arrange
            var heap = CreateHeapInstance();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => heap.Peek());
            Assert.Throws<InvalidOperationException>(() => heap.Pop());
        }

        [Theory]
        [MemberData(nameof(GetHeapConfigurations))]
        public void Elements_should_be_popped_correctly(HeapConfiguration conf)
        {
            var iterations = conf.CalculateIterations(5000);
            for (var seed = 0; seed < iterations; ++seed)
            {
                // Arrange
                var keys = conf.GenerateKeys(new Random(seed));
                var heap = CreateHeapInstance();
                var count = 0;

                foreach (var key in keys)
                {
                    Assert.Equal(count, heap.Count);
                    heap.Add(key, Guid.NewGuid().ToString());
                    Assert.Equal(++count, heap.Count);
                }

                // Act & Assert
                this.Pop_until_empty(heap, keys);
            }
        }

        private void Pop_until_empty(THeap heap, IReadOnlyCollection<int> keys)
        {
            var count = keys.Count;

            foreach (var key in keys.OrderBy(x => x))
            {
                Assert.False(heap.IsEmpty);
                Assert.Equal(key, heap.Peek().Key);

                Assert.Equal(count, heap.Count);
                Assert.Equal(key, heap.Pop().Key);
                Assert.Equal(--count, heap.Count);
            }

            Assert.True(heap.IsEmpty);
        }

        [Theory]
        [MemberData(nameof(GetHeapConfigurations))]
        public void Elements_should_be_updated_correctly(HeapConfiguration conf)
        {
            for (var seed = 0; seed < 15; ++seed)
            {
                // Arrange
                var random = new Random(seed);
                var keys = conf.GenerateKeys(random);
                var heap = CreateHeapInstance();

                var handles = keys
                    .Select(k => new HandleKeyPair(heap.Add(k, Guid.NewGuid().ToString()), k))
                    .ToList();

                for (var i = 0; i < 15000; ++i)
                {
                    var handleIndex = random.Next(0, conf.HeapSize);
                    var newKey = random.Next(conf.KeyLimit);
                    var handle = handles[handleIndex];

                    // Act
                    heap.Update(handle.Node, newKey);
                    handles[handleIndex].Key = newKey;

                    // Assert
                    Assert.Equal(Peek(handles), heap.Peek().Key);
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetHeapConfigurations))]
        public void All_elements_should_be_removed_correctly(HeapConfiguration conf)
        {
            var iterations = conf.CalculateIterations(5000);
            for (var seed = 0; seed < iterations; ++seed)
            {
                // Arrange
                var random = new Random(seed);
                var keys = conf.GenerateKeys(random);
                var heap = CreateHeapInstance();
                var count = conf.HeapSize;

                var handles = keys
                    .Select(k => new HandleKeyPair(heap.Add(k, Guid.NewGuid().ToString()), k))
                    .ToList();

                // Act & Assert
                while (!heap.IsEmpty)
                {
                    Assert.Equal(Peek(handles), heap.Peek().Key);
                    Assert.Equal(count, heap.Count);

                    var handle = handles[random.Next(count--)];
                    handles.Remove(handle);
                    heap.Remove(handle.Node);

                    if (count == 0)
                    {
                        Assert.Throws<InvalidOperationException>(() => heap.Peek());
                    }
                    else
                    {
                        Assert.Equal(Peek(handles), heap.Peek().Key);
                        Assert.Equal(count, heap.Count);
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetHeapConfigurations))]
        public void Elements_should_be_removed_correctly(HeapConfiguration conf)
        {
            var iterations = conf.CalculateIterations(5000);
            for (var seed = 0; seed < iterations; ++seed)
            {
                // Arrange
                var random = new Random(seed);
                var keys = conf.GenerateKeys(random);

                for (var toRemove = 0; toRemove < conf.HeapSize; ++toRemove)
                {
                    var heap = CreateHeapInstance();
                    var count = conf.HeapSize;

                    var handles = keys
                        .Select(k => new HandleKeyPair(heap.Add(k, Guid.NewGuid().ToString()), k))
                        .ToList();

                    // Act & Assert

                    // Part I - remove one node
                    Assert.Equal(Peek(handles), heap.Peek().Key);
                    Assert.Equal(count--, heap.Count);

                    heap.Remove(handles[toRemove].Node);
                    handles.RemoveAt(toRemove);

                    if (count == 0)
                    {
                        Assert.Throws<InvalidOperationException>(() => heap.Peek());
                    }
                    else
                    {
                        Assert.Equal(Peek(handles), heap.Peek().Key);
                        Assert.Equal(count, heap.Count);
                    }

                    // Part II - pop until empty
                    while (!heap.IsEmpty)
                    {
                        var min = handles.MinBy(x => x.Key);
                        handles.Remove(min);

                        Assert.Equal(min.Key, heap.Pop().Key);
                        Assert.Equal(--count, heap.Count);
                    }

                    Assert.True(heap.IsEmpty);
                    Assert.Equal(0, count);
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetHeapConfigurations))]
        public void Heaps_should_be_merged_correctly(HeapConfiguration conf)
        {
            var iterations = conf.CalculateIterations(5000);
            for (var seed = 0; seed < iterations; ++seed)
            {
                // Arrange
                var random = new Random(seed);
                var keys1 = conf.GenerateKeys(random);
                var secondHeapSize = random.Next(conf.HeapSize/2, conf.HeapSize*2);
                var keys2 = conf.GenerateKeys(random, secondHeapSize);

                var heap1 = CreateHeapInstance();
                var heap2 = CreateHeapInstance();

                foreach (var key in keys1)
                    heap1.Add(key, Guid.NewGuid().ToString());

                foreach (var key in keys2)
                    heap2.Add(key, Guid.NewGuid().ToString());

                var mixedKeys = keys1.Concat(keys2).ToList();

                // Arrange
                heap1.Merge(heap2);

                // Assert
                this.Pop_until_empty(heap1, mixedKeys);
            }
        }

        [Fact]
        public void Enumerating_empty_heap_should_yield_empty_collection()
        {
            // Arrange
            var heap = CreateHeapInstance();

            // Assert
            Assert.Empty(heap);
        }

        [Theory]
        [MemberData(nameof(GetHeapConfigurations))]
        public void Heap_enumeration_should_yield_all_elements(HeapConfiguration conf)
        {
            var iterations = conf.CalculateIterations(1000);
            for (var seed = 0; seed < iterations; ++seed)
            {
                // Arrange
                var random = new Random(seed);
                var keys = conf.GenerateKeys(random);
                var heap = CreateHeapInstance();

                var handles = keys
                    .Select(k => heap.Add(k, Guid.NewGuid().ToString()));

                var expected = handles
                    .Select(x => new KeyValuePair<int, string>(x.Key, x.Value))
                    .OrderBy(x => x.Value)
                    .ToArray();

                // Act
                var actual = heap
                    .Select(x => new KeyValuePair<int, string>(x.Key, x.Value))
                    .OrderBy(x => x.Value)
                    .ToArray();

                // Assert
                Assert.True(expected.SequenceEqual(actual));
            }
        }
    }
}
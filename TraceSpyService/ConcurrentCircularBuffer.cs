using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TraceSpyService
{
    // This buffer contains a maximum fixed set of items. The set is circular, old items are automatically removed.
    // The user can get items by an absolute incremental index (a long), but items may have been lost between calls too distant in time.
    // This class is fully thread-safe.
    public sealed class ConcurrentCircularBuffer<T> : ICollection<T>
    {
        private T[] _items;
        private int _index;
        private bool _full;

        public ConcurrentCircularBuffer(int capacity)
        {
            if (capacity <= 1) // need at least two items
                throw new ArgumentException(null, nameof(capacity));

            Capacity = capacity;
            _items = new T[capacity];
        }

        public int Capacity { get; private set; }
        public long TotalCount { get; private set; }

        public int Count
        {
            get
            {
                lock (SyncObject) // full & _index need to be in sync
                {
                    return _full ? Capacity : _index;
                }
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                return;

            lock (SyncObject)
            {
                foreach (var item in items)
                {
                    AddWithLock(item);
                }
            }
        }

        private long AddWithLock(T item)
        {
            _items[_index] = item;
            _index++;
            if (_index == Capacity)
            {
                _full = true;
                _index = 0;
            }
            long index = TotalCount++;
            return index;
        }

        public void Add(T item)
        {
            AddAndGetIndex(item);
        }

        public long AddAndGetIndex(T item)
        {
            lock (SyncObject)
            {
                return AddWithLock(item);
            }
        }

        public void Clear()
        {
            lock (SyncObject)
            {
                _items = new T[Capacity];
                _index = 0;
                _full = false;
                TotalCount = 0;
            }
        }

        // this gives raw access to the underlying buffer. not sure I should keep that
        public T this[int index]
        {
            get
            {
                return _items[index];
            }
        }

        public T[] GetTail(long startIndex)
        {
            return GetTail(startIndex, long.MaxValue, true);
        }

        public T[] GetTail(long startIndex, long maxCount)
        {
            return GetTail(startIndex, maxCount, true);
        }

        public T[] GetTail(long startIndex, long maxCount, bool throwOnError)
        {
            long totalCount;
            return GetTail(startIndex, maxCount, throwOnError, out totalCount);
        }

        public T[] GetTail(long startIndex, long maxCount, bool throwOnError, out long totalCount)
        {
            long lostCount;
            return GetTail(startIndex, maxCount, throwOnError,  out totalCount, out lostCount);
        }

        public T[] GetTail(long startIndex, long maxCount, bool throwOnError, out long totalCount, out long lostCount)
        {
            if (startIndex < 0)
            {
                if (throwOnError)
                    throw new ArgumentOutOfRangeException(nameof(startIndex));

                lostCount = -1;
                totalCount = -1;
                return null;
            }

            T[] array = ToArray(out totalCount, out int count);
            if (startIndex > totalCount)
            {
                if (throwOnError)
                    throw new ArgumentOutOfRangeException(nameof(startIndex));

                lostCount = -1;
                return null;
            }

            lostCount = (totalCount - count) - startIndex;
            if (lostCount >= 0)
                return array;

            lostCount = 0;

            // this maybe could optimized to not allocate the initial array
            // but in multi-threading environment, I suppose this is arguable (and more difficult).
            T[] chunk = new T[Math.Min(totalCount - startIndex, maxCount)];
            if (chunk.Length > 0)
            {
                Array.Copy(array, array.Length - (totalCount - startIndex), chunk, 0, chunk.Length);
            }
            return chunk;
        }

        public T[] ToArray(out long totalCount, out int count)
        {
            lock (SyncObject)
            {
                T[] items = new T[_full ? Capacity : _index];
                if (_full)
                {
                    if (_index == 0)
                    {
                        Array.Copy(_items, items, Capacity);
                    }
                    else
                    {
                        Array.Copy(_items, _index, items, 0, Capacity - _index);
                        Array.Copy(_items, 0, items, Capacity - _index, _index);
                    }
                }
                else if (_index > 0)
                {
                    Array.Copy(_items, items, _index);
                }
                totalCount = TotalCount;
                count = Count;
                return items;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ToArray(out long totalCount, out int count).AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        bool ICollection<T>.Contains(T item)
        {
            return _items.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (array.Rank != 1)
                throw new ArgumentException(null, nameof(array));

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            T[] thisArray = ToArray(out long totalCount, out int count);
            if ((array.Length - arrayIndex) < count)
                throw new ArgumentException(null, nameof(array));

            Array.Copy(thisArray, 0, array, arrayIndex, thisArray.Length);
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool ICollection<T>.Remove(T item)
        {
            return false;
        }

        private static object _syncObject;
        private static object SyncObject
        {
            get
            {
                if (_syncObject == null)
                {
                    object obj = new object();
                    Interlocked.CompareExchange(ref _syncObject, obj, null);
                }
                return _syncObject;
            }
        }
    }
}

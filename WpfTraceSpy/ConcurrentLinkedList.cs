using System.Collections.Generic;
using System.Threading;

namespace TraceSpy
{
    public class ConcurrentLinkedList<T>
    {
        private readonly List<T> _list = new List<T>();

        private static object _syncObject;
        private static object SyncObject
        {
            get
            {
                if (_syncObject == null)
                {
                    var obj = new object();
                    Interlocked.CompareExchange(ref _syncObject, obj, null);
                }
                return _syncObject;
            }
        }

        public void Add(T item)
        {
            lock (SyncObject)
            {
                _list.Add(item);
            }
        }

        public T[] GetAndClear()
        {
            T[] array;
            lock (SyncObject)
            {
                array = _list.ToArray();
                _list.Clear();
            }
            return array;
        }
    }
}

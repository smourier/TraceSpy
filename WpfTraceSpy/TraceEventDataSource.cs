using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace TraceSpy
{
    public class TraceEventDataSource : IList<TraceEvent>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private List<TraceEvent> _list = new List<TraceEvent>();

        public int Count => _list.Count;

        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        private void OnCollectionReset() => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index) => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item, index));
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index) => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));

        public TraceEvent this[int index]
        {
            get => _list[index];
            set
            {
                var oldItem = this[index];
                _list[index] = value;
                OnPropertyChanged("Item[]");
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem, value, index);
            }
        }

        public void Add(TraceEvent item)
        {
            int index = Count;
            _list.Add(item);
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public void Clear()
        {
            _list.Clear();
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
            OnCollectionReset();
        }

        public void Insert(int index, TraceEvent item)
        {
            _list.Insert(index, item);
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public bool Remove(TraceEvent item)
        {
            int index = _list.IndexOf(item);
            if (index < 0)
                return false;

            _list.RemoveAt(index);
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            return true;
        }

        public void RemoveAt(int index)
        {
            var item = this[index];
            _list.RemoveAt(index);
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        public bool Contains(TraceEvent item) => _list.Contains(item);
        public int IndexOf(TraceEvent item) => _list.IndexOf(item);
        public IEnumerator<TraceEvent> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        bool ICollection<TraceEvent>.IsReadOnly => false;
        void ICollection<TraceEvent>.CopyTo(TraceEvent[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
    }
}

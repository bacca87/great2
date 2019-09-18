using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Great.Utils
{
    public class MRUCollection<T> : ObservableCollection<T>
    {
        public int MaxSize { get; private set; }

        public MRUCollection(int maxSize, Collection<T> collection = null)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentException("The MaxSize parameter must be greater than 0!", "MaxSize");
            }

            MaxSize = maxSize;

            if (collection != null)
            {
                foreach (T item in collection.Take(maxSize))
                {
                    base.Add(item);
                }
            }
        }

        public new void Add(T obj) => Insert(0, obj);

        protected override void InsertItem(int index, T item)
        {
            if (index >= MaxSize)
            {
                throw new ArgumentOutOfRangeException("index", index, $"Cannot insert more than {MaxSize} ");
            }

            if (Contains(item))
            {
                Remove(item);
            }

            if (Count >= MaxSize)
            {
                RemoveAt(Count - 1);
            }

            base.InsertItem(index, item);
        }
    }
}

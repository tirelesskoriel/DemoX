using System.Collections;
using System.Collections.Generic;

namespace DemoX.Framework.Core
{
    public class OrderedUniqueSet<T>
    {
        private readonly HashSet<T> _uniqueSet = new();
        private readonly List<T> _insertionOrder = new();

        public void Add(T item)
        {
            if (_uniqueSet.Add(item))
            {
                _insertionOrder.Add(item);
            }
        }

        public void Remove(T item)
        {
            if (_uniqueSet.Remove(item))
            {
                _insertionOrder.Remove(item);
            }
        }

        public bool Contains(T item)
        {
            return _uniqueSet.Contains(item);
        }

        public int Count => _uniqueSet.Count;

        public bool IsEmpty => _uniqueSet.Count == 0;

        public bool IsNotEmpty => _uniqueSet.Count != 0;

        public IEnumerable<T> GetInsertionOrder()
        {
            return _insertionOrder;
        }

    }
}
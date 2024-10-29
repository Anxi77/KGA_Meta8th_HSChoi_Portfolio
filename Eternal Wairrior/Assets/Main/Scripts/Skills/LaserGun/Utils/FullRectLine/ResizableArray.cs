using System;
using System.Collections;
using System.Collections.Generic;

namespace LaserSystem2D
{
    public class ResizableArray<T> : IList<T>
    {
        public T[] Items;

        public ResizableArray()
        {
            Items = Array.Empty<T>();
        }

        public ResizableArray(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }
        
            Items = new T[capacity];
        }

        public T this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            EnsureCapacity(Count + 1);
            Items[Count++] = item;
        }
    
        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            if (collection is ICollection<T> c)
            {
                EnsureCapacity(Count + c.Count);
                c.CopyTo(Items, Count);
                Count += c.Count;
            }
            else
            {
                foreach (var item in collection)
                {
                    Add(item);
                }
            }
        }

        public void Clear()
        {
            Array.Clear(Items, 0, Count);
            Count = 0;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (arrayIndex < 0 || arrayIndex + Count > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            Array.Copy(Items, 0, array, arrayIndex, Count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return Items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(T item)
        {
            return Array.IndexOf(Items, item, 0, Count);
        }

        public void Insert(int index, T item)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            EnsureCapacity(Count + 1);
            Array.Copy(Items, index, Items, index + 1, Count - index);
            Items[index] = item;
            Count++;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index == -1)
            {
                return false;
            }
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            Array.Copy(Items, index + 1, Items, index, Count - index - 1);
            Count--;
        }

        private void EnsureCapacity(int capacity)
        {
            if (capacity > Items.Length)
            {
                int newCapacity = Math.Max(Items.Length * 2, capacity);
                Array.Resize(ref Items, newCapacity);
            }
        }
    }
}
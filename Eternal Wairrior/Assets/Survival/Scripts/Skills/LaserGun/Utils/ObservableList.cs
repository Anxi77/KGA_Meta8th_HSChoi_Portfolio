using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    [Serializable]
    public class ObservableList<T> : IList<T>
    {
        [SerializeField] private List<T> _list;
        
        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public delegate void ChangedDelegate(int index, T oldValue, T newValue);
        public event ChangedDelegate ValueChanged;
        public event Action Updated;

        public ObservableList(List<T> list)
        {
            _list = list;
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _list.Add(item);
            Updated?.Invoke();
        }

        public void Clear()
        {
            _list.Clear();
            Updated?.Invoke();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            bool output = _list.Remove(item);
            Updated?.Invoke();

            return output;
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            _list.InsertRange(index, collection);
            Updated?.Invoke();
        }

        public void AddRange(IEnumerable<T> collection)
        {
            _list.AddRange(collection);
            Updated?.Invoke();
        }

        public void RemoveAll(Predicate<T> predicate)
        {
            _list.RemoveAll(predicate);
            Updated?.Invoke();
        }

        public void RemoveRange(int index, int count)
        {
            _list.RemoveRange(index, count);
            Updated?.Invoke();
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            Updated?.Invoke();
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
            Updated?.Invoke();
        }

        public T this[int index]
        {
            get => _list[index];
            set
            {
                T oldValue = _list[index];
                _list[index] = value;
                ValueChanged?.Invoke(index, oldValue, value);
                Updated?.Invoke();
            }
        }
    }
}
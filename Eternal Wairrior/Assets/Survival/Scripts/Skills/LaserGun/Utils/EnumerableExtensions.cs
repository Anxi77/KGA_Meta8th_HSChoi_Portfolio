using System;
using System.Collections.Generic;

namespace LaserSystem2D
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T element in collection)
            {
                action(element);
            }
        }

        public static List<T> ReserveList<T>(int count) where T : new()
        {
            List<T> list = new();
            
            while (count -- > 0)
            {
                list.Add(new T());
            }

            return list;
        }
        
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
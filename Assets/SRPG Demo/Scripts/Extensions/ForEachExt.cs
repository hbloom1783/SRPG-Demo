using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SRPGDemo.Extensions
{
    public static class ForEachExt
    {
        public static void ForEach<T>(this IEnumerable<T> target, Action<T> action)
        {
            foreach (T item in target)
            {
                action(item);
            }
        }

        /*public static void ForEach<K, V>(this Dictionary<K, V> target, Action<KeyValuePair<K, V>> action)
        {
            foreach(KeyValuePair<K, V> pair in target)
            {
                action(pair);
            }
        }*/
    }
}

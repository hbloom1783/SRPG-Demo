using System;
using System.Collections.Generic;

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
    }
}

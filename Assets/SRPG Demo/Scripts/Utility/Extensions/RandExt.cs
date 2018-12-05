using System;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace SRPGDemo.Extensions
{
    public static class RandExt
    {
        public static T RandomPick<T>(this IEnumerable<T> input)
        {
            return input.ToList().RandomPick();
        }

        public static T RandomPick<T>(this ICollection<T> input)
        {
            return input.ElementAt(Random.Range(0, input.Count - 1));
        }

        public static T RandomTake<T>(this ICollection<T> input)
        {
            T item = input.RandomPick();
            input.Remove(item);
            return item;
        }

        public static T RandomPickMin<T>(this IEnumerable<T> input, Func<T, int> selector)
        {
            int minVal = input.Min(selector);

            return input.Where(x => selector(x) == minVal).RandomPick();
        }

        public static T RandomTakeMin<T>(this ICollection<T> input, Func<T, int> selector)
        {
            T item = input.RandomPickMin(selector);
            input.Remove(item);
            return item;
        }

        public static T RandomPickMax<T>(this IEnumerable<T> input, Func<T, int> selector)
        {
            int maxVal = input.Max(selector);

            return input.Where(x => selector(x) == maxVal).RandomPick();
        }

        public static T RandomTakeMax<T>(this ICollection<T> input, Func<T, int> selector)
        {
            T item = input.RandomPickMax(selector);
            input.Remove(item);
            return item;
        }
    }
}
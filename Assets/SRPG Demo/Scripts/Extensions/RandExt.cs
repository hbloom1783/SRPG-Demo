using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace SRPGDemo.Extensions
{
    public static class RandExt
    {
        public static T RandomPick<T>(this IEnumerable<T> input)
        {
            List<T> list = new List<T>(input);
            return list[Random.Range(0, list.Count - 1)];
        }

        public static T RandomPick<T>(this ICollection<T> input)
        {
            List<T> list = new List<T>(input);
            return list[Random.Range(0, list.Count - 1)];
        }

        public static T RandomTake<T>(this ICollection<T> input)
        {
            T item = input.RandomPick();
            input.Remove(item);
            return item;
        }
    }
}
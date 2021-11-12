using System;
using System.Collections.Generic;

namespace CombatTest.Utils
{
    public static class ListExtensions
    {
        public static T GetRandomElement<T>(this IList<T> list, Random rng)
        {
            return list[rng.Next(list.Count)];
        }
    }
}

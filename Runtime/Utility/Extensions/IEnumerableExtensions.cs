using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Konfus_Systems_Tools_n_Utils_Package.Runtime.Utility.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T GetRandom<T>(this IEnumerable<T> enumerable)
        {
            var array = enumerable.ToArray();
            var enumCount = array.Count();
            var rand = Random.CreateFromIndex((uint)enumCount);
            return array.ElementAt(rand.NextInt(enumCount));
        }
    }
}
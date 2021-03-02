using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> UnionIfNotNull<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
            {
                first = Enumerable.Empty<T>();
            }
            if (second != null)
            {
                return first.Union(second);
            }
            return first;
        }
    }
}

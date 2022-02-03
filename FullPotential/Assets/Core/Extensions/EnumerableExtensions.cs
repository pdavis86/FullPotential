using System.Collections.Generic;
using System.Linq;

// ReSharper disable ConvertIfStatementToNullCoalescingAssignment

namespace FullPotential.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> UnionIfNotNull<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
            {
                first = Enumerable.Empty<T>();
            }

            return second != null
                ? first.Union(second)
                : first;
        }
    }
}

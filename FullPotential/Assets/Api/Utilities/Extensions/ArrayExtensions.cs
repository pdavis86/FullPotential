using System;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class ArrayExtensions
    {
        public static int IndexOf<T>(this T[] source, T element)
        {
            return Array.IndexOf(source, element);
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<T1, T2> OrderByValue<T1, T2>(this Dictionary<T1, T2> dictionary)
        {
            return dictionary.OrderBy(kvp => kvp.Value)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static string GetStringValueOrNull<T1>(this Dictionary<T1, string> dictionary, T1 key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null;
        }
    }
}

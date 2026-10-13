using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<T1, T2> OrderByValue<T1, T2>(this Dictionary<T1, T2> dictionary)
        {
            return dictionary
                .OrderBy(kvp => kvp.Value)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static string GetStringValueOrNull<T>(this Dictionary<T, string> dictionary, T key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : null;
        }

        public static int GetIntValue<T>(this Dictionary<T, string> dictionary, T key)
        {
            return dictionary.TryGetValue(key, out var value) && value != null ? int.Parse(value) : 0;
        }

        public static bool GetBoolValue<T>(this Dictionary<T, string> dictionary, T key)
        {
            return dictionary.TryGetValue(key, out var value) && value != null && bool.Parse(value);
        }

        public static List<SerializableKeyValuePair<string, string>> GetSerializableKeyValuePairList(this Dictionary<string, string> dictionary)
        {
            return dictionary
                .Select(kvp => new SerializableKeyValuePair<string, string>(kvp.Key, kvp.Value))
                .ToList();
        }
    }
}

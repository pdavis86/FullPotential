using System.Collections.Generic;
using System.Linq;

using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class SerializableKeyValuePairExtensions
    {
        public static T2 GetValue<T1, T2>(this List<SerializableKeyValuePair<T1, T2>> list, T1 key)
        {
            return list.FirstOrDefault(x => x.Key.Equals(key)).Value;
        }

        public static void SetValue<T1, T2>(this List<SerializableKeyValuePair<T1, T2>> list, T1 key, T2 value)
        {
            var index = list.FindIndex(x => x.Key.Equals(key));
            if (index >= 0)
            {
                var item = list[index];
                item.Value = value;
                list[index] = item;
            }
            else
            {
                list.Add(new SerializableKeyValuePair<T1, T2>(key, value));
            }
        }
    }
}

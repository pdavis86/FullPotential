using System.Collections.Generic;

using FullPotential.Api.Data.Models;

using NUnit.Framework;

using Unity.Netcode;

namespace FullPotential.Api.Unity.Extensions
{
    public static class SerializationExtensions
    {
        public static void WriteValueSafe(this FastBufferWriter writer, in SerializableKeyValuePair<string, string> kvp)
        {
            writer.WriteValueSafe($"{kvp.Key}={kvp.Value}");
        }

        public static void ReadValueSafe(this FastBufferReader reader, out SerializableKeyValuePair<string, string> kvp)
        {
            reader.ReadValueSafe(out string val);
            var split = val.Split(new char[] { '=' });
            kvp = new SerializableKeyValuePair<string, string>(split[0], split[1]);
        }

        public static void WriteValueSafe(this FastBufferWriter writer, in SerializableKeyValuePair<string, string>[] kvpArray)
        {
            foreach (var kvp in kvpArray)
            {
                writer.WriteValueSafe($"{kvp.Key}={kvp.Value}");
            }
        }

        public static void ReadValueSafe(this FastBufferReader reader, out SerializableKeyValuePair<string, string>[] kvpArray)
        {
            var results = new List<SerializableKeyValuePair<string, string>>();
            do
            {
                reader.ReadValueSafe(out string val);
                var split = val.Split(new char[] { '=' });
                results.Add(new SerializableKeyValuePair<string, string>(split[0], split[1]));
            } while (reader.Position < reader.Length);

            kvpArray = results.ToArray();
        }
    }
}

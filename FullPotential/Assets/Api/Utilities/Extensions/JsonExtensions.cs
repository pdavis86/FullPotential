using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializer Serializer = JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.Indented });

        private static readonly StringBuilder Builder = new();

        private static readonly StringWriter Writer = new(Builder);

        public static string ToJson(this object model)
        {
            Serializer.Serialize(Writer, model);
            var json = Writer.ToString();
            Reset();
            return json;
        }

        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static void Reset()
        {
            Builder.Clear();
            if (Builder.Capacity > 65536)
            {
                Builder.Capacity = 1024;
            }
        }
    }
}

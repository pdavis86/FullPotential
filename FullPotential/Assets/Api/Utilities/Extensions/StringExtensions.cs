// ReSharper disable UnusedMember.Global

using System.Text;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static string OrIfNullOrWhitespace(this string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value)
                ? fallback
                : value;
        }

        public static string EmptyIfNull(this string value)
        {
            return value ?? string.Empty;
        }

        public static string ToSpacedString(this string value)
        {
            var builder = new StringBuilder();
            foreach (var c in value)
            {
                if (char.IsUpper(c) && builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(c);
            }
            return builder.ToString();
        }
    }
}

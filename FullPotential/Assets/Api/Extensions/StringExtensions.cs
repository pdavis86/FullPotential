namespace FullPotential.Api.Extensions
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

    }
}

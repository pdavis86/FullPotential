namespace Assets.Extensions
{
    public static class StringExtensions
    {
        public static string OrIfNullOrWhitespace(this string original, string replacement)
        {
            return string.IsNullOrWhiteSpace(original)
                ? replacement
                : original;
        }
    }
}

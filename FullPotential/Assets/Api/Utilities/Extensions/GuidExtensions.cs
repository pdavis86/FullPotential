using System;

namespace FullPotential.Api.Utilities.Extensions
{
    public static class GuidExtensions
    {
        public static string ToMinimisedString(this Guid guid)
        {
            return Convert.ToBase64String(guid.ToByteArray());
        }
    }
}
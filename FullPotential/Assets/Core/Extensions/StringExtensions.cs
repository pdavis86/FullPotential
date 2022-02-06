using System;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Core.Extensions
{
    public static class StringExtensions
    {
        public static Guid ToUnminimisedGuid(this string minimisedString)
        {
            return new Guid(Convert.FromBase64String(minimisedString));
        }

    }
}

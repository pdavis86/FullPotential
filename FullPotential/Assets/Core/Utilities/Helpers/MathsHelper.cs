using System;

namespace FullPotential.Core.Utilities.Helpers
{
    public static class MathsHelper
    {
        public static int GetMinBiasedNumber(int min, int max, Random random)
        {
            return min + (int)Math.Round((max - min) * Math.Pow(random.NextDouble(), 3), 0);
        }

    }
}

using System;

namespace FullPotential.Assets.Core.Helpers
{
    public static class MathsHelper
    {
        public static int GetMinBiasedNumber(int min, int max, Random random)
        {
            return min + (int)Math.Round((max - min) * Math.Pow(random.NextDouble(), 3), 0);
        }

        public static bool AreRoughlyEqual(float value1, float value2)
        {
            var rounded1 = UnityEngine.Mathf.RoundToInt(value1 * 100);
            var rounded2 = UnityEngine.Mathf.RoundToInt(value2 * 100);

            return rounded1 == rounded2;
        }

    }
}

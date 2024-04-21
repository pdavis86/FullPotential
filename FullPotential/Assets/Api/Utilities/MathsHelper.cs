namespace FullPotential.Api.Utilities
{
    public static class MathsHelper
    {
        public static float GetHighInHighOutInRange(int value, float resultMin, float resultMax, int valueMax = 100)
        {
            return value / (float)valueMax * (resultMax - resultMin) + resultMin;
        }

        public static float GetHighInLowOutInRange(int value, float resultMin, float resultMax, int valueMax = 100)
        {
            return (valueMax + 1 - value) / (float)valueMax * (resultMax - resultMin) + resultMin;
        }
    }
}

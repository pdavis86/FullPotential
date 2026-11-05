using System;
using System.Threading;

namespace FullPotential.Api.Utilities
{
    public static class ThreadSafeRandom
    {
        private static readonly ThreadLocal<Random> _localRandom = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        public static int Next()
        {
            return _localRandom.Value.Next();
        }

        public static int Next(int maxValue)
        {
            return _localRandom.Value.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            return _localRandom.Value.Next(minValue, maxValue);
        }

        public static double NextDouble()
        {
            return _localRandom.Value.NextDouble();
        }
    }
}

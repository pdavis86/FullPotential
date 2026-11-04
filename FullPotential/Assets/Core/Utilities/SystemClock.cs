using System;
using System.Diagnostics;

using FullPotential.Api.Utilities;

namespace FullPotential.Core.Utilities
{
    public class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public long GetTimestamp() => Stopwatch.GetTimestamp();

        public double GetElapsedSeconds(long timestamp) => (double)(Stopwatch.GetTimestamp() - timestamp) / Stopwatch.Frequency;
    }
}

using System;

namespace FullPotential.Api.Utilities
{
    public interface IClock
    {
        DateTime UtcNow { get; }

        long GetTimestamp();

        double GetElapsedSeconds(long timestamp);
    }
}

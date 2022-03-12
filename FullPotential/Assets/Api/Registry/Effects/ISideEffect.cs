using System;

namespace FullPotential.Api.Registry.Effects
{
    public interface ISideEffect
    {
        /// <summary>
        /// This effect occurs as a result of applying another effect
        /// </summary>
        Type SideEffectOf { get; }
    }
}

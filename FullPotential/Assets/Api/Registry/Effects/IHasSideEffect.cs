using System;

namespace FullPotential.Api.Registry.Effects
{
    public interface IHasSideEffect
    {
        /// <summary>
        /// This effect occurs as a result of applying another effect
        /// </summary>
        Type SideEffectType { get; }
    }
}

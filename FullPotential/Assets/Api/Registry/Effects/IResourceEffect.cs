using System;
using FullPotential.Api.Gameplay.Effects;

namespace FullPotential.Api.Registry.Effects
{
    public interface IResourceEffect : IEffect
    {
        /// <summary>
        /// The underlying result of applying this effect
        /// </summary>
        AffectType AffectType { get; }

        /// <summary>
        /// The resource which will be affected
        /// </summary>
        Guid ResourceTypeId { get; }
    }
}

using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Registry.Effects
{
    public interface IStatEffect : IEffect
    {
        /// <summary>
        /// The underlying result of applying this effect
        /// </summary>
        AffectType AffectType { get; }

        /// <summary>
        /// The stat which will be affected
        /// </summary>
        ResourceType StatToAffect { get; }
    }
}

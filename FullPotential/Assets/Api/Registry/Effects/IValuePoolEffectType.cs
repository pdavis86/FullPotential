using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;

namespace FullPotential.Api.Registry.Effects
{
    public interface IValuePoolEffectType : IEffectType
    {
        /// <summary>
        /// The underlying result of applying this effect
        /// </summary>
        EffectActionType EffectActionType { get; }

        /// <summary>
        /// The Id of the valuePool type which will be affected
        /// </summary>
        ValuePoolTypeId ValuePoolTypeId { get; }
    }
}

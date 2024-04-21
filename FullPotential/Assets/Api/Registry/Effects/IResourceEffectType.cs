using FullPotential.Api.Gameplay.Effects;

namespace FullPotential.Api.Registry.Effects
{
    public interface IResourceEffectType : IEffectType
    {
        /// <summary>
        /// The underlying result of applying this effect
        /// </summary>
        EffectActionType EffectActionType { get; }

        /// <summary>
        /// The Id of the resource type which will be affected
        /// </summary>
        string ResourceTypeIdString { get; }
    }
}

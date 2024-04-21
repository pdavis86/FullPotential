using FullPotential.Api.Gameplay.Effects;

namespace FullPotential.Api.Registry.Effects
{
    public interface IMovementEffectType : IEffectType
    {
        /// <summary>
        /// The direction in which the source/target will be moved
        /// </summary>
        MovementDirection Direction { get; }
    }
}

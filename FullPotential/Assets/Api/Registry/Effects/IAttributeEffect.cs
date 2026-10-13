namespace FullPotential.Api.Registry.Effects
{
    public interface IAttributeEffect : IEffectType
    {
        /// <summary>
        /// Whether the maximum will be temporarily increased (otherwise decreased)
        /// </summary>
        bool IsTemporaryMaxIncrease { get; }

        /// <summary>
        /// The ID of the attribute which will be affected
        /// </summary>
        string AttributeId { get; }
    }
}

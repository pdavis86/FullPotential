namespace FullPotential.Api.Registry.Effects
{
    public interface IAttributeEffect : IEffect
    {
        /// <summary>
        /// The attribute which will be affected
        /// </summary>
        string AttributeToAffect { get; }
    }
}

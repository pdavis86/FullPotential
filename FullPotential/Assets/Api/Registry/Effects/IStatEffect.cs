namespace FullPotential.Api.Registry.Effects
{
    public interface IStatEffect : IEffect
    {
        /// <summary>
        /// The underlying result of applying this effect
        /// </summary>
        Affect Affect { get; }

        /// <summary>
        /// The stat which will be affected
        /// </summary>
        AffectableStats StatToAffect { get; }
    }
}

namespace FullPotential.Api.Registry.Effects
{
    public interface IStatEffect : IEffect
    {
        /// <summary>
        /// The stat which will be affected
        /// </summary>
        AffectableStats? StatToAffect { get; }
    }
}

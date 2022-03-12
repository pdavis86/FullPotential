namespace FullPotential.Api.Registry.Effects
{
    public interface IEffect : IRegisterable
    {
        /// <summary>
        /// The underlying result of applying this effect
        /// </summary>
        Affect Affect { get; }
    }
}

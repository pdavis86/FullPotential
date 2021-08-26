namespace FullPotential.Assets.Api.Registry
{
    public interface IEffect : IRegisterable
    {
        /// <summary>
        /// Set this to true if this status effect is a side effect of another effect
        /// </summary>
        bool IsSideEffect { get; }
    }
}

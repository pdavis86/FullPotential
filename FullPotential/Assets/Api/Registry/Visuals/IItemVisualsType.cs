namespace FullPotential.Api.Registry.Visuals
{
    public interface IItemVisualsType : IRegisterableType, IHasPrefab
    {
        string ApplicableToTypeIdString { get; }
    }
}

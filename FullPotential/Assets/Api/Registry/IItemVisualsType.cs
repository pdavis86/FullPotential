namespace FullPotential.Api.Registry
{
    public interface IItemVisualsType : IRegisterableType, IHasPrefab
    {
        string ApplicableToTypeIdString { get; }
    }
}

namespace FullPotential.Api.Registry
{
    public interface IItemVisuals : IRegisterableType, IHasPrefab
    {
        string ApplicableToTypeIdString { get; }
    }
}

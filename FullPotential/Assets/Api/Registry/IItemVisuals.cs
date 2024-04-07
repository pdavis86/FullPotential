namespace FullPotential.Api.Registry
{
    public interface IItemVisuals : IRegisterable, IHasPrefab
    {
        string ApplicableToTypeIdString { get; }
    }
}

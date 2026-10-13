namespace FullPotential.Api.Registry
{
    // todo: rename with Type suffix
    public interface IItemVisuals : IRegisterableType, IHasPrefab
    {
        string ApplicableToTypeIdString { get; }
    }
}

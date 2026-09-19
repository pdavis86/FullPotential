namespace FullPotential.Api.Registry
{
    public interface IItemStackType : IRegisterableType
    {
        int MaxStackSize { get; }

        int MinDropCount { get; }

        int MaxDropCount { get; }
    }
}

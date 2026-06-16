namespace FullPotential.Api.Registry
{
    public interface IItemStackType : IRegisterableType
    {
        public int MaxStackSize { get; }

        public int MinDropCount { get; }

        public int MaxDropCount { get; }
    }
}

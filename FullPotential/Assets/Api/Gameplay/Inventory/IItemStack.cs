namespace FullPotential.Api.Gameplay.Inventory
{
    public interface IItemStack
    {
        public int MaxStackSize { get; }

        public int MinDropCount { get; }

        public int MaxDropCount { get; }
    }
}

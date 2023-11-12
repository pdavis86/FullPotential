using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Gameplay.Inventory
{
    public class ItemStack : ItemBase
    {
        public int Count { get; set; }

        public int MaxSize => ((IItemStack)RegistryType).MaxStackSize;
    }
}

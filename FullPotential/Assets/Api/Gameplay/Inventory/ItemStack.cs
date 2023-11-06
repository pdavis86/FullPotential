using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.Weapons;

namespace FullPotential.Api.Gameplay.Inventory
{
    public class ItemStack : ItemBase
    {
        public int Count { get; set; }

        public int MaxSize => ((IAmmunition) RegistryType).MaxStackSize;
    }
}

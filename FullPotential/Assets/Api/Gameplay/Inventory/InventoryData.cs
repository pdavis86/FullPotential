using FullPotential.Api.Items.Types;
using FullPotential.Api.Utilities.Data;

namespace FullPotential.Api.Gameplay.Inventory
{
    [System.Serializable]
    public class InventoryData
    {
        public int MaxItems;

        public Loot[] Loot;
        public Accessory[] Accessories;
        public Armor[] Armor;
        public Weapon[] Weapons;
        public Consumer[] Consumers;
        public ItemStack[] ItemStacks;
        public SpecialGear[] SpecialGear;

        public KeyValuePair<string, string>[] EquippedItems;
        public KeyValuePair<string, string>[] ShapeMapping;
    }
}

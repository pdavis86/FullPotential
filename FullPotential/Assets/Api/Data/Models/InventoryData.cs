using System;

using FullPotential.Api.Items.Types;

namespace FullPotential.Api.Data.Models
{
    [Serializable]
    public class InventoryData
    {
        public string Username;
        public int MaxItems;

        public Loot[] Loot;
        public Accessory[] Accessories;
        public Armor[] Armor;
        public Weapon[] Weapons;
        public Consumer[] Consumers;
        public ItemStack[] ItemStacks;
        public SpecialGear[] SpecialGear;

        public SerializableKeyValuePair<string, string>[] EquippedItems;
        public SerializableKeyValuePair<string, string>[] ShapeMapping;
    }
}

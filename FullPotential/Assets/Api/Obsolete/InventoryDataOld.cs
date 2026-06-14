using System;

using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Api.Obsolete
{
    [Serializable]
    public class InventoryDataOld
    {
        public string Username;
        public int MaxItems;

        public Loot[] Loot;
        public Accessory[] Accessories;
        public Armor[] Armor;
        public Weapon[] Weapons;
        public Consumer[] Consumers;
        public ItemStackBase[] ItemStacks;
        public SpecialGear[] SpecialGear;

        public SerializableKeyValuePair<string, string>[] EquippedItems;
        public SerializableKeyValuePair<string, string>[] ShapeMapping;
    }
}

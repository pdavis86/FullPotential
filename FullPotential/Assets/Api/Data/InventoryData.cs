using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Api.Data
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

        public SerializableKeyValuePair<string, string>[] EquippedItems;
        public SerializableKeyValuePair<string, string>[] ShapeMapping;

        public IEnumerable<ItemBase> GetEquipableItems()
        {
            return Enumerable.Empty<ItemBase>()
                .UnionIfNotNull(Accessories)
                .UnionIfNotNull(Armor)
                .UnionIfNotNull(Consumers)
                .UnionIfNotNull(SpecialGear)
                .UnionIfNotNull(Weapons);
        }

        public IEnumerable<ItemBase> GetNonItemStacks()
        {
            return Enumerable.Empty<ItemBase>()
                .UnionIfNotNull(Accessories)
                .UnionIfNotNull(Armor)
                .UnionIfNotNull(Consumers)
                .UnionIfNotNull(Loot)
                .UnionIfNotNull(SpecialGear)
                .UnionIfNotNull(Weapons);
        }

        public IEnumerable<ItemBase> GetAllItems()
        {
            return Enumerable.Empty<ItemBase>()
                 .UnionIfNotNull(Accessories)
                 .UnionIfNotNull(Armor)
                 .UnionIfNotNull(Consumers)
                 .UnionIfNotNull(ItemStacks)
                 .UnionIfNotNull(Loot)
                 .UnionIfNotNull(SpecialGear)
                 .UnionIfNotNull(Weapons);
        }
    }
}

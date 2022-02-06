using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Loot;
using FullPotential.Api.Registry.Spells;
using FullPotential.Core.Data;

namespace FullPotential.Api.Data
{
    [System.Serializable]
    public class InventoryData
    {
        public int MaxItems;

        public Loot[] Loot;
        public Accessory[] Accessories;
        public Armor[] Armor;
        public Spell[] Spells;
        public Weapon[] Weapons;

        public KeyValuePair<string, string>[] EquippedItems;
    }
}

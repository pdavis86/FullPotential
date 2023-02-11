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
        public Gadget[] Gadgets;
        public Spell[] Spells;
        public Weapon[] Weapons;

        public KeyValuePair<string, string>[] EquippedItems;
        public KeyValuePair<string, string>[] ShapeMapping;
    }
}

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

        public KeyValuePair<string, string>[] EquippedItems;
        public KeyValuePair<string, string>[] ShapeMapping;

        //todo: zzz v0.5 - remove gadgets and spells
        public Consumer[] Gadgets;
        public Consumer[] Spells;
    }
}

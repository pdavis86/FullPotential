using Assets.Core.Crafting;
using System;

namespace Assets.Core.Data
{
    [Serializable]
    public class Inventory
    {
        public int MaxItems;

        public ItemBase[] Loot;
        public Accessory[] Accessories;
        public Armor[] Armor;
        public Spell[] Spells;
        public Weapon[] Weapons;

        public string[] EquipSlots;
    }
}

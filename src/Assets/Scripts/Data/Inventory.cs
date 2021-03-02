using Assets.Scripts.Crafting.Results;
using System;

namespace Assets.Scripts.Data
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
    }
}

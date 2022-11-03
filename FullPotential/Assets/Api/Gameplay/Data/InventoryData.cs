﻿using FullPotential.Api.Items;
using FullPotential.Api.Items.SpellsAndGadgets;
using FullPotential.Api.Items.Weapons;
using FullPotential.Api.Utilities.Data;

namespace FullPotential.Api.Gameplay.Data
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
        public WeaponItemBase[] Weapons;

        public KeyValuePair<string, string>[] EquippedItems;
    }
}

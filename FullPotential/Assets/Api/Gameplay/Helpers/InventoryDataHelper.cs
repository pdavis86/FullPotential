﻿using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.SpellsAndGadgets;

namespace FullPotential.Api.Gameplay.Helpers
{
    public class InventoryDataHelper
    {
        public void PopulateInventoryChangesWithItem(InventoryChanges invChanges, ItemBase item)
        {
            var itemType = item.GetType();
            invChanges.Accessories = itemType == typeof(Accessory) ? new[] { item as Accessory } : null;
            invChanges.Armor = itemType == typeof(Armor) ? new[] { item as Armor } : null;
            invChanges.Gadgets = itemType == typeof(Gadget) ? new[] { item as Gadget } : null;
            invChanges.Spells = itemType == typeof(Spell) ? new[] { item as Spell } : null;
            invChanges.Weapons = itemType == typeof(Weapon) ? new[] { item as Weapon } : null;
        }
    }
}

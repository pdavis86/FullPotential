using FullPotential.Api.Data;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Gear;
using FullPotential.Api.Registry.Spells;

namespace FullPotential.Api.Helpers
{
    public static class InventoryDataHelper
    {
        public static void PopulateInventoryChangesWithItem(InventoryChanges invChanges, ItemBase item)
        {
            var itemType = item.GetType();
            invChanges.Accessories = itemType == typeof(Accessory) ? new[] { item as Accessory } : null;
            invChanges.Armor = itemType == typeof(Armor) ? new[] { item as Armor } : null;
            invChanges.Spells = itemType == typeof(Spell) ? new[] { item as Spell } : null;
            invChanges.Weapons = itemType == typeof(Weapon) ? new[] { item as Weapon } : null;
        }
    }
}

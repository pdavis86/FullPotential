using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.SpellsAndGadgets;
using FullPotential.Api.Items.Weapons;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Api.Gameplay.Data
{
    public class InventoryDataService : IInventoryDataService
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

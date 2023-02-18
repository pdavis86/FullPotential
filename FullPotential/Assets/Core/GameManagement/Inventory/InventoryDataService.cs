using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.GameManagement.Inventory
{
    public class InventoryDataService : IInventoryDataService
    {
        public void PopulateInventoryChangesWithItem(InventoryChanges invChanges, ItemBase item)
        {
            var itemType = item.GetType();
            invChanges.Accessories = itemType == typeof(Accessory) ? new[] { item as Accessory } : null;
            invChanges.Armor = itemType == typeof(Armor) ? new[] { item as Armor } : null;
            invChanges.Consumers = itemType == typeof(Consumer) ? new[] { item as Consumer } : null;
            invChanges.Weapons = itemType == typeof(Weapon) ? new[] { item as Weapon } : null;
        }
    }
}

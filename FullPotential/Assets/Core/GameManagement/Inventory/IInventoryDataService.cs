using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Items.Base;

namespace FullPotential.Core.GameManagement.Inventory
{
    public interface IInventoryDataService
    {
        void PopulateInventoryChangesWithItem(InventoryChanges invChanges, ItemBase item);
    }
}
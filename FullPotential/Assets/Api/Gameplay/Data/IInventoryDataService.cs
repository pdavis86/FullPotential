using FullPotential.Api.Registry.Base;

namespace FullPotential.Api.Gameplay.Data
{
    public interface IInventoryDataService
    {
        void PopulateInventoryChangesWithItem(InventoryChanges invChanges, ItemBase item);
    }
}
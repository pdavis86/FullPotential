using System.Collections.Generic;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Gameplay.Enums;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.Gear;

namespace FullPotential.Api.Gameplay.Inventory
{
    public interface IPlayerInventory : IInventory
    {
        List<string> ValidateIsCraftable(string[] componentIds, ItemBase itemToCraft);

        IEnumerable<ItemBase> GetCompatibleItemsForSlot(IGear.GearCategory? gearCategory);

        KeyValuePair<SlotGameObjectName, EquippedItem>? GetEquippedWithItemId(string itemId);

        InventoryData GetSaveData();

        bool IsInventoryFull();
    }
}

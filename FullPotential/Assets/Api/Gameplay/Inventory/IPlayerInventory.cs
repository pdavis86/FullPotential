using System.Collections.Generic;
using FullPotential.Api.Gameplay.Data;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.Crafting;

namespace FullPotential.Api.Gameplay.Inventory
{
    public interface IPlayerInventory : IInventory
    {

        List<ItemBase> GetComponentsFromIds(string[] componentIds);

        List<string> ValidateIsCraftable(string[] componentIds, ItemBase itemToCraft);

        IEnumerable<ItemBase> GetCompatibleItemsForSlot(IGear.GearCategory? gearCategory);

        KeyValuePair<SlotGameObjectName, EquippedItem>? GetEquippedWithItemId(string itemId);

        InventoryData GetSaveData();

        bool IsInventoryFull();
    }
}

using System.Collections.Generic;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete;

namespace FullPotential.Api.Gameplay.Inventory
{
    public interface IPlayerInventory : IInventory
    {
        List<ItemBase> GetComponentsFromIds(string[] componentIds);

        List<string> ValidateIsCraftable(string[] componentIds, ItemBase itemToCraft);

        IEnumerable<ItemBase> GetCompatibleItemsForSlot(GearCategory? gearCategory);

        KeyValuePair<SlotGameObjectName, EquippedItem>? GetEquippedWithItemId(string itemId);

        InventoryData GetSaveData();

        bool IsInventoryFull();

        string GetAssignedShape(string itemId);

        bool SetAssignedShape(string itemId, string shape);

        ItemBase GetItemFromAssignedShape(string shape);
    }
}

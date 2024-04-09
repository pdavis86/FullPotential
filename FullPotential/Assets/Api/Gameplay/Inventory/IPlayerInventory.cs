using System.Collections.Generic;
using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Gameplay.Inventory
{
    public interface IPlayerInventory
    {
        List<CombatItemBase> GetComponentsFromIds(string[] componentIds);

        List<string> ValidateIsCraftable(string[] componentIds, ItemBase itemToCraft);

        IEnumerable<ItemBase> GetHandItems();

        IEnumerable<ItemBase> GetCompatibleItems(string slotId);

        KeyValuePair<string, EquippedItem>? GetEquippedWithItemId(string itemId);

        bool IsInventoryFull();

        string GetAssignedShape(string itemId);

        bool SetAssignedShape(string itemId, string shape);

        ItemBase GetItemFromAssignedShape(string shape);
    }
}

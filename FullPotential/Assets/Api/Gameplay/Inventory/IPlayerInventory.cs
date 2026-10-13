using System.Collections.Generic;

using FullPotential.Api.Gameplay.Player;
using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Gameplay.Inventory
{
    public interface IPlayerInventory : IInventory
    {
        IEnumerable<ItemBase> GetHandItems();

        IEnumerable<ItemBase> GetCompatibleItems(string slotId);

        KeyValuePair<string, EquippedItem>? GetEquippedWithItemId(string itemId);

        string GetAssignedShape(string itemId);

        bool SetAssignedShape(string itemId, string shape);

        ItemBase GetItemFromAssignedShape(string shape);
    }
}

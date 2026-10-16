using System.Collections.Generic;

using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Gameplay.Inventory
{
    public interface IPlayerInventory : IInventory
    {
        IEnumerable<ItemBase> GetHandItems();

        IEnumerable<ItemBase> GetCompatibleItems(string slotId);

        string GetAssignedShape(string itemId);

        bool SetAssignedShape(string itemId, string shape);

        ItemBase GetItemFromAssignedShape(string shape);

        Dictionary<string, string> GetEquippedItems();
    }
}

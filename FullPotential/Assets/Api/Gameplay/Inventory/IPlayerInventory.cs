using System.Collections.Generic;

using FullPotential.Api.Items.Base;

using FullPotential.Api.CoreTypeIds;

namespace FullPotential.Api.Gameplay.Inventory
{
    public interface IPlayerInventory : IInventory
    {
        IEnumerable<ItemBase> GetHandItems();

        IEnumerable<ItemBase> GetCompatibleItems(SlotId? slotId);

        string GetAssignedShape(string itemId);

        bool SetAssignedShape(string itemId, string shape);

        ItemBase GetItemFromAssignedShape(string shape);

        Dictionary<SlotId, string> GetEquippedItems();
    }
}

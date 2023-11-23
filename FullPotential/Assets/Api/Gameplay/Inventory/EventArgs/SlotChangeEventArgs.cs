using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Inventory.EventArgs
{
    public class SlotChangeEventArgs : IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public InventoryBase Inventory { get; }

        public string SlotId { get; }

        public string ItemId { get; }

        public SlotChangeEventArgs(InventoryBase inventory, string slotId, string itemId)
        {
            Inventory = inventory;
            SlotId = slotId;
            ItemId = itemId;
        }
    }
}

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Inventory.EventArgs
{
    public class SlotChangeEventArgs : IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public IInventory Inventory { get; }

        public string SlotId { get; }

        public string ItemId { get; }

        public SlotChangeEventArgs(IInventory inventory, string slotId, string itemId)
        {
            Inventory = inventory;
            SlotId = slotId;
            ItemId = itemId;
        }
    }
}

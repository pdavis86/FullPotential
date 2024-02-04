using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Inventory.EventArgs
{
    public class SlotChangeEventArgs : IEventHandlerArgs
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public InventoryBase Inventory { get; }

        public LivingEntityBase LivingEntity { get; }

        public string SlotId { get; }

        public string ItemId { get; }

        public SlotChangeEventArgs(InventoryBase inventory, LivingEntityBase livingEntity, string slotId, string itemId)
        {
            Inventory = inventory;
            LivingEntity = livingEntity;
            SlotId = slotId;
            ItemId = itemId;
        }
    }
}

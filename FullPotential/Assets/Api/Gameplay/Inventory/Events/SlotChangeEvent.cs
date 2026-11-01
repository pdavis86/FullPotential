using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Behaviours;

using FullPotential.Api.CoreTypeIds;

namespace FullPotential.Api.Gameplay.Inventory.Events
{
    public readonly struct SlotChangeEvent : IEvent
    {
        public InventoryBase Inventory { get; }

        public LivingEntityBase LivingEntity { get; }

        public SlotId SlotId { get; }

        public string ItemId { get; }

        public SlotChangeEvent(InventoryBase inventory, LivingEntityBase livingEntity, SlotId slotId, string itemId)
        {
            Inventory = inventory;
            LivingEntity = livingEntity;
            SlotId = slotId;
            ItemId = itemId;
        }
    }
}

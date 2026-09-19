using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Inventory.Events
{
    [RegisterEvent("9c7972de-4136-4825-aaa3-11925ad049ee")]
    public class SlotChangeEvent : IEvent
    {
        public bool IsDefaultHandlerCancelled { get; set; }

        public InventoryBase Inventory { get; }

        public LivingEntityBase LivingEntity { get; }

        public string SlotId { get; }

        public string ItemId { get; }

        public SlotChangeEvent(InventoryBase inventory, LivingEntityBase livingEntity, string slotId, string itemId)
        {
            Inventory = inventory;
            LivingEntity = livingEntity;
            SlotId = slotId;
            ItemId = itemId;
        }
    }
}

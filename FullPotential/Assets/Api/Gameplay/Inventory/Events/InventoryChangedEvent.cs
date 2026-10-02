using System.Collections.Generic;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Gameplay.Inventory.Events
{
    public readonly struct InventoryChangedEvent : IEvent
    {
        public InventoryBase Inventory { get; }

        public IReadOnlyList<ItemBase> ItemsAdded { get; }

        public IReadOnlyList<ItemBase> ItemsRemoved { get; }

        public InventoryChangedEvent(
            InventoryBase inventory,
            IReadOnlyList<ItemBase> itemsAdded,
            IReadOnlyList<ItemBase> itemsRemoved)
        {
            Inventory = inventory;
            ItemsAdded = itemsAdded;
            ItemsRemoved = itemsRemoved;
        }
    }
}

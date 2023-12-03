using System.Collections.Generic;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Items.Base;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Enemies.Behaviours
{
    public class EnemyInventory : InventoryBase
    {
        protected override void SetEquippedItem(string itemId, string slotId)
        {
            //Nothing here
        }

        protected override void ApplyEquippedItemChanges(Api.Data.SerializableKeyValuePair<string, string>[] equippedItems)
        {
            //Nothing here
        }

        protected override void NotifyOfItemsAdded(IEnumerable<ItemBase> itemsAdded)
        {
            //Nothing here
        }

        protected override void NotifyOfInventoryFull()
        {
            //Nothing here
        }

        protected override void NotifyOfItemsRemoved(IEnumerable<ItemBase> itemsRemoved)
        {
            //Nothing here
        }
    }
}

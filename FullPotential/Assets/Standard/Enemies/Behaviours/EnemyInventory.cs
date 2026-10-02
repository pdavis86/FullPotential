using System.Collections.Generic;

using FullPotential.Api.Gameplay.Behaviours;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Enemies.Behaviours
{
    public class EnemyInventory : InventoryBase
    {
        public override void ApplyEquippedItemChange(string itemId, string slotId)
        {
            //Nothing here
        }

        protected override void ApplyEquippedItemChanges(Dictionary<string, string> equippedItems)
        {
            //Nothing here
        }
    }
}

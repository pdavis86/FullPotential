using System.Collections.Generic;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Behaviours;

namespace FullPotential.Standard.Enemies.Behaviours
{
    public class EnemyInventory : InventoryBase
    {
        public override void ApplyEquippedItemChange(string itemId, SlotId slotId)
        {
            //Nothing here
        }

        protected override void ApplyEquippedItemChanges(Dictionary<string, string> equippedItems)
        {
            //Nothing here
        }
    }
}

using System.Collections.Generic;

using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete.Items;
using FullPotential.Api.Obsolete.Items.Base;

namespace FullPotential.Api.Gameplay.Crafting
{
    public interface IResultFactory
    {
        ItemBase GetLootDrop();

        ItemBase GetAmmoDrop();

        ItemBase GetCraftedItem(
            CraftableType craftableType,
            string typeId,
            string resourceTypeId,
            bool isTwoHanded,
            IList<CombatItemBase> components);
    }
}
using System.Collections.Generic;
using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Gameplay.Crafting
{
    public interface IResultFactory
    {
        ItemBase GetLootDrop();

        ItemBase GetAmmoDrop();

        ItemBase GetCraftedItem(CraftableType craftableType, string subTypeName, bool isTwoHanded, IList<ItemForCombatBase> components);
    }
}
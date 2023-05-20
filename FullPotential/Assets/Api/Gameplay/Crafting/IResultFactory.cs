using System.Collections.Generic;
using FullPotential.Api.Items.Base;

namespace FullPotential.Api.Gameplay.Crafting
{
    public interface IResultFactory
    {
        ItemBase GetLootDrop();
        ItemBase GetCraftedItem(string categoryName, string typeName, bool isTwoHanded, IList<ItemBase> components);
    }
}
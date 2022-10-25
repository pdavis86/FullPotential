using System.Collections.Generic;
using FullPotential.Api.Registry.Base;
using FullPotential.Api.Registry.SpellsAndGadgets;

namespace FullPotential.Core.Gameplay.Crafting
{
    public interface IResultFactory
    {
        ITargeting GetTargeting(string typeName);
        IShape GetShape(string typeName);
        ItemBase GetLootDrop();
        ItemBase GetCraftedItem(string categoryName, string typeName, bool isTwoHanded, IEnumerable<ItemBase> components);
        string GetItemDescription(ItemBase item, bool includeNameAndType = true, string itemName = null);
    }
}
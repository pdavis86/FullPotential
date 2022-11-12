using System.Collections.Generic;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.SpellsAndGadgets;

namespace FullPotential.Api.Gameplay.Crafting
{
    public interface IResultFactory
    {
        ITargeting GetTargeting(string typeId);
        IShape GetShape(string typeName);
        ItemBase GetLootDrop();
        ItemBase GetCraftedItem(string categoryName, string typeName, bool isTwoHanded, IEnumerable<ItemBase> components);

        //Obsolete
        ITargeting GetTargetingFromTypeName(string typeName);
        IShape GetShapeFromTypeName(string typeName);
    }
}
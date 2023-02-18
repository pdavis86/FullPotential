using System.Collections.Generic;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.Consumers;

namespace FullPotential.Api.Gameplay.Crafting
{
    public interface IResultFactory
    {
        ITargeting GetTargeting(string typeId);
        IShape GetShape(string typeName);
        ItemBase GetLootDrop();
        ItemBase GetCraftedItem(string categoryName, string typeName, bool isTwoHanded, IList<ItemBase> components);

        //Obsolete
        ITargeting GetTargetingFromTypeName(string typeName);
        IShape GetShapeFromTypeName(string typeName);
    }
}
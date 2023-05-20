using System.Collections.Generic;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Registry.Targeting;

namespace FullPotential.Api.Gameplay.Crafting
{
    public interface IResultFactory
    {
        IShape GetShape(string typeName);
        ItemBase GetLootDrop();
        ItemBase GetCraftedItem(string categoryName, string typeName, bool isTwoHanded, IList<ItemBase> components);
    }
}
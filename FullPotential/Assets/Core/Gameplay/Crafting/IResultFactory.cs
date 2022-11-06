﻿using System.Collections.Generic;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.SpellsAndGadgets;

namespace FullPotential.Core.Gameplay.Crafting
{
    public interface IResultFactory
    {
        ITargeting GetTargeting(string typeName);
        IShape GetShape(string typeName);
        ItemBase GetLootDrop();
        ItemBase GetCraftedItem(string categoryName, string typeName, bool isTwoHanded, IEnumerable<ItemBase> components);
    }
}
using System;
using System.Collections.Generic;

using FullPotential.Api.Items;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry;

namespace FullPotential.Api.Gameplay.Crafting
{
    public interface IResultFactory
    {
        ItemBase GetLootDrop();

        ItemBase GetAmmoDrop();

        ItemBase GetCraftedItem(
            Type craftableType,
            string typeId,
            string resourceTypeId,
            bool isTwoHanded,
            string itemName,
            IList<ItemBase> components);

        int ComputeAttribute(IList<ItemBase> components, string attributeId, bool allowMax = true);

        void SetTargetingAndShape(IHasTargetingAndShape item, IList<ItemBase> components);

        void SetEffects(IHasEffects item, IList<ItemBase> components);

        void SetItemName(ItemBase item, bool isAttack, string customSuffix = null);
    }
}

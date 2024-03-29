﻿using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Resources
{
    public class Mana : IResource
    {
        public Guid TypeId => ResourceTypeIds.Mana;

        public Color Color => Color.FromArgb(185, 36, 158);

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => "Core/Prefabs/Combat/SpellInHand.prefab";

        public Action<LivingEntityBase> ReplenishBehaviour => PerformReplenish;

        private void PerformReplenish(LivingEntityBase livingEntity)
        {
            if (!livingEntity.IsConsumingResource(ResourceTypeIds.ManaId)
                && livingEntity.GetResourceValue(ResourceTypeIds.ManaId) < livingEntity.GetResourceMax(ResourceTypeIds.ManaId))
            {
                //todo: zzz v0.5 - trait-based resource recharge
                livingEntity.AdjustResourceValue(ResourceTypeIds.ManaId, 1);
            }
        }
    }
}

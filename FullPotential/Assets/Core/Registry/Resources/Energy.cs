using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Resources
{
    public class Energy : IResource
    {
        public Guid TypeId => ResourceTypeIds.Energy;

        public Color Color => Color.FromArgb(25, 118, 64);

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => "Core/Prefabs/Combat/GadgetInHand.prefab";

        public Action<LivingEntityBase> ReplenishBehaviour => PerformReplenish;

        private void PerformReplenish(LivingEntityBase livingEntity)
        {
            if (!livingEntity.IsConsumingResource(ResourceTypeIds.EnergyId)
                && livingEntity.GetResourceValue(ResourceTypeIds.EnergyId) < livingEntity.GetResourceMax(ResourceTypeIds.EnergyId))
            {
                //todo: zzz v0.5 - trait-based resource recharge
                livingEntity.AdjustResourceValue(ResourceTypeIds.EnergyId, 1);
            }
        }
    }
}

using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Resources
{
    public class Stamina : IResource
    {
        public Guid TypeId => ResourceTypeIds.Stamina;

        public Color Color => Color.FromArgb(39, 80, 147);

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => null;

        public Action<LivingEntityBase> ReplenishBehaviour => PerformReplenish;

        private void PerformReplenish(LivingEntityBase livingEntity)
        {
            if (!livingEntity.IsConsumingResource(ResourceTypeIds.StaminaId)
                && livingEntity.GetResourceValue(ResourceTypeIds.StaminaId) < livingEntity.GetResourceMax(ResourceTypeIds.StaminaId))
            {
                //todo: zzz v0.5 - trait-based resource recharge
                livingEntity.AdjustResourceValue(ResourceTypeIds.StaminaId, 1);
            }
        }
    }
}

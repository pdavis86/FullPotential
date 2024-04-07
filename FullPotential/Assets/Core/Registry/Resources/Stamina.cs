using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Resources
{
    public class Stamina : IResource
    {
        private static readonly Guid Id = new Guid(ResourceTypeIds.StaminaId);
        private static readonly Color ResourceColor = Color.FromArgb(39, 80, 147);

        public Guid TypeId => Id;

        public Color Color => ResourceColor;

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => null;

        public Action<LivingEntityBase> ReplenishBehaviour => PerformReplenish;

        private void PerformReplenish(LivingEntityBase livingEntity)
        {
            if (!livingEntity.IsConsumingResource(ResourceTypeIds.StaminaId)
                && livingEntity.GetResourceValue(ResourceTypeIds.StaminaId) < livingEntity.GetResourceMax(ResourceTypeIds.StaminaId))
            {
                //todo: zzz v0.4 - trait-based resource recharge
                livingEntity.AdjustResourceValue(ResourceTypeIds.StaminaId, 1);
            }
        }
    }
}

using System;
using System.Drawing;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Core.Registry.ValuePools
{
    public class Stamina : IValuePoolType
    {
        private static readonly Guid Id = new Guid(ValuePoolTypeIds.StaminaId);
        private static readonly Color ValuePoolBackgroundColor = Color.FromArgb(25, 118, 64); // Green
        private static readonly Color ValuePoolTextColor = Color.FromArgb(255, 255, 255); // White

        public Guid TypeId => Id;

        public Color BackgroundColor => ValuePoolBackgroundColor;

        public Color TextColor => ValuePoolTextColor;

        public bool IsCraftable => true;

        public string ItemInHandDefaultPrefab => null;

        public Action<LivingEntityBase> ReplenishBehaviour => PerformReplenish;

        private void PerformReplenish(LivingEntityBase livingEntity)
        {
            if (!livingEntity.IsConsumingValuePool(ValuePoolTypeIds.StaminaId)
                && livingEntity.GetValuePoolValue(ValuePoolTypeIds.StaminaId) < livingEntity.GetValuePoolMax(ValuePoolTypeIds.StaminaId))
            {
                //todo: zzz v0.8 - trait-based valuePool recharge
                livingEntity.TriggerValuePoolValueUpdate(ValuePoolTypeIds.StaminaId, 1, livingEntity.name, null);
            }
        }
    }
}

using System;
using System.Drawing;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Logging;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Standard.CustomGear.Barrier;
using FullPotential.Standard.SpecialSlots;

namespace FullPotential.Standard.ValuePools
{
    public class BarrierChargeValuePool : IValuePoolType
    {
        public const string TypeIdString = "9f026e17-d313-4402-9da6-c5b002e26c64";

        private static readonly Color ValuePoolBackgroundColor = Color.FromArgb(185, 36, 158); // Purple
        private static readonly Color ValuePoolTextColor = Color.FromArgb(255, 255, 255); // White

        private readonly IAuditor _logger;

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public Color BackgroundColor => ValuePoolBackgroundColor;

        public Color TextColor => ValuePoolTextColor;

        public bool IsCraftable => false;

        public string ItemInHandDefaultPrefab => null;

        public Action<LivingEntityBase> ReplenishBehaviour => PerformReplenish;

        public BarrierChargeValuePool(IAuditorFactory auditorFactory)
        {
            _logger = auditorFactory.Create(this);
        }

        private void PerformReplenish(LivingEntityBase livingEntity)
        {
            if (livingEntity.GetValuePoolValue(TypeIdString) >= livingEntity.GetValuePoolMax(TypeIdString))
            {
                return;
            }

            if (livingEntity is not FighterBase targetFighter)
            {
                _logger.Error("LivingEntity was not a fighter. Cannot replenish barrier valuePool");
                return;
            }

            var equippedBarrier = targetFighter.Inventory.GetItemInSlot<Items.CustomGear>(SlotId.FromString(BarrierSlot.TypeIdString));

            if (equippedBarrier == null)
            {
                return;
            }

            var lastHit = equippedBarrier.GetCustomData(HealthChangeEventHandler.CustomDataKeyLastHit);

            if (!lastHit.IsNullOrWhiteSpace())
            {
                var rechargeDelay = Barrier.GetRechargeDelay(equippedBarrier);

                if (DateTime.TryParse(lastHit, out var lastHitDateTime)
                    && (DateTime.UtcNow - lastHitDateTime.ToUniversalTime()).TotalSeconds < rechargeDelay)
                {
                    return;
                }
            }

            //Consume item valuePool (-2 to overcome replenish of 1)
            livingEntity.TriggerValuePoolValueUpdate(equippedBarrier.ValuePoolType.TypeId.ToString(), -2, targetFighter.name, null);

            //Replenish barrier valuePool
            //todo: zzz v0.8 - trait-based valuePool recharge
            livingEntity.TriggerValuePoolValueUpdate(TypeIdString, Barrier.GetRechargeRate(equippedBarrier), targetFighter.name, null);
        }
    }
}

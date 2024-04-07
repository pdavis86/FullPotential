using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Standard.SpecialGear.Barrier;
using FullPotential.Standard.SpecialSlots;

namespace FullPotential.Standard.Resources
{
    public class BarrierChargeResource : IResource
    {
        public const string TypeIdString = "9f026e17-d313-4402-9da6-c5b002e26c64";

        private static readonly Guid Id = new Guid(TypeIdString);
        private static readonly Color ResourceColor = Color.FromArgb(255, 255, 0);

        public Guid TypeId => Id;

        public Color Color => ResourceColor;

        public bool IsCraftable => false;

        public string ItemInHandDefaultPrefab => null;

        public Action<LivingEntityBase> ReplenishBehaviour => PerformReplenish;

        private void PerformReplenish(LivingEntityBase livingEntity)
        {
            if (livingEntity.GetResourceValue(TypeIdString) >= livingEntity.GetResourceMax(TypeIdString))
            {
                return;
            }

            if (livingEntity is not FighterBase targetFighter)
            {
                UnityEngine.Debug.LogError("LivingEntity was not a fighter. Cannot replenish barrier resource");
                return;
            }

            var equippedBarrier = (Api.Items.Types.SpecialGear)targetFighter.Inventory.GetItemInSlot(BarrierSlot.TypeIdString);

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

            //Consume item resource (-2 to overcome replenish of 1)
            livingEntity.AdjustResourceValue(equippedBarrier.ResourceTypeId, -2);

            //Replenish barrier resource
            //todo: zzz v0.4 - trait-based resource recharge
            livingEntity.AdjustResourceValue(TypeIdString, Barrier.GetRechargeRate(equippedBarrier));
        }
    }
}

using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Standard.SpecialGear.Barrier;
using FullPotential.Standard.SpecialSlots;

namespace FullPotential.Standard.Resources
{
    public class BarrierChargeResource : IResource
    {
        public const string TypeIdString = "9f026e17-d313-4402-9da6-c5b002e26c64";

        public Guid TypeId => new Guid(TypeIdString);

        public Color Color => Color.FromArgb(255, 255, 0);

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

            var lastHit = equippedBarrier.GetCustomData(DamageDealtEventHandler.CustomDataKeyLastHit);

            if (!lastHit.IsNullOrWhiteSpace())
            {
                var rechargeDelay = equippedBarrier.GetRechargeDelay();

                if (DateTime.TryParse(lastHit, out var lastHitDateTime)
                    && (DateTime.UtcNow - lastHitDateTime.ToUniversalTime()).TotalSeconds < rechargeDelay)
                {
                    return;
                }
            }

            //Consume item resource (-2 to overcome replenish of 1)
            livingEntity.AdjustResourceValue(equippedBarrier.ResourceTypeId, -2);

            //Replenish barrier resource
            //todo: zzz v0.5 - trait-based resource recharge
            livingEntity.AdjustResourceValue(TypeIdString, equippedBarrier.GetRechargeRate());
        }
    }
}

using System;
using System.Drawing;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
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

            var barrier = (Api.Items.Types.SpecialGear)targetFighter.Inventory.GetItemInSlot(BarrierSlot.TypeIdString);

            if (barrier == null)
            {
                return;
            }

            //todo: delay
            var rechargeDelay = barrier.GetRechargeDelay();
            //int.Parse(barrier.GetCustomData(CustomDataBarrierCharge).OrIfNullOrWhitespace("0"));
            //private const string CustomDataBarrierCharge = "ChargeValue";

            //Consume item resource (-2 to overcome replenish of 1)
            livingEntity.AdjustResourceValue(barrier.ResourceTypeId, -2);

            //Replenish barrier resource
            //todo: zzz v0.5 - trait-based resource recharge
            var rechargeRate = barrier.GetRechargeRate();
            livingEntity.AdjustResourceValue(TypeIdString, rechargeRate);
        }
    }
}

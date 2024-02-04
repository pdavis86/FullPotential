using System;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Standard.Resources;
using FullPotential.Standard.SpecialSlots;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class DamageDealtEventHandler : IEventHandler
    {
        public const string CustomDataKeyLastHit = "LastHit";

        public NetworkLocation Location => NetworkLocation.Server;

        public Action<IEventHandlerArgs> BeforeHandler => HandleBeforeDamageDealt;

        public Action<IEventHandlerArgs> AfterHandler => null;

        private void HandleBeforeDamageDealt(IEventHandlerArgs eventArgs)
        {
            var healthChangeArgs = (DamageDealtEventArgs)eventArgs;

            if (healthChangeArgs.Change >= 0)
            {
                return;
            }

            var barrier = (Api.Items.Types.SpecialGear)healthChangeArgs.LivingEntity.Inventory.GetItemInSlot(BarrierSlot.TypeIdString);

            if (barrier == null)
            {
                return;
            }

            barrier.SetCustomData(CustomDataKeyLastHit, DateTime.UtcNow.ToString("u"));

            var barrierCharge = healthChangeArgs.LivingEntity.GetResourceValue(BarrierChargeResource.TypeIdString);

            if (barrierCharge <= 0)
            {
                //Debug.Log("Barrier depleted. Taking full damage");
                return;
            }

            healthChangeArgs.LivingEntity.AdjustResourceValue(BarrierChargeResource.TypeIdString, healthChangeArgs.Change);

            if (barrierCharge < Math.Abs(healthChangeArgs.Change))
            {
                //Debug.Log("Barrier nearly depleted. Taking partial damage");
                healthChangeArgs.Change += barrierCharge;
                return;
            }

            //Debug.Log("Barrier OK. Taking no damage");
            eventArgs.IsDefaultHandlerCancelled = true;
        }
    }
}

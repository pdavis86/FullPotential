using System;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Standard.Resources;
using FullPotential.Standard.SpecialSlots;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class HealthChangeEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Action<IEventHandlerArgs> BeforeHandler => HandleBeforeHealthChange;

        public Action<IEventHandlerArgs> AfterHandler => null;

        private void HandleBeforeHealthChange(IEventHandlerArgs eventArgs)
        {
            var healthChangeArgs = (HealthChangeEventArgs)eventArgs;

            if (healthChangeArgs.Change >= 0)
            {
                return;
            }

            if (healthChangeArgs.LivingEntity is not FighterBase targetFighter)
            {
                return;
            }

            var barrier = (Api.Items.Types.SpecialGear)targetFighter.Inventory.GetItemInSlot(BarrierSlot.TypeIdString);

            if (barrier == null)
            {
                return;
            }

            var barrierCharge = targetFighter.GetResourceValue(barrier.ResourceTypeId);

            if (barrierCharge <= 0)
            {
                //todo: remove
                Debug.Log("Barrier depleted. Taking full damage");

                return;
            }

            healthChangeArgs.LivingEntity.AdjustResourceValue(BarrierChargeResource.TypeIdString, healthChangeArgs.Change);

            if (barrierCharge < Math.Abs(healthChangeArgs.Change))
            {
                //todo: remove
                Debug.Log("Barrier nearly depleted. Taking partial damage");

                healthChangeArgs.Change += barrierCharge;
                return;
            }

            //todo: remove
            Debug.Log("Barrier OK. Taking no damage");

            eventArgs.IsDefaultHandlerCancelled = true;
        }
    }
}

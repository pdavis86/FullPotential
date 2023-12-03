using System;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Items.Types;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Accessories.Barrier
{
    public class DamageTakenEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Action<IEventHandlerArgs> BeforeHandler => HandleBeforeDamageTaken;

        public Action<IEventHandlerArgs> AfterHandler => HandleAfterDamageTaken;

        private void HandleBeforeDamageTaken(IEventHandlerArgs eventArgs)
        {
            //todo: finish BarrierEventHandler
            Debug.Log("Before taking damage");

            var damageEventArgs = (DamageTakenEventArgs)eventArgs;

            var entity = damageEventArgs.LivingEntity;

            if (entity is not FighterBase fighter)
            {
                return;
            }

            var slotId = Accessory.GetSlotId(Barrier.TypeIdString, 1);
            var barrier = (Accessory)fighter.Inventory.GetItemInSlot(slotId);

            if (barrier == null)
            {
                return;
            }

            Debug.Log("Got here!!!");
        }

        private void HandleAfterDamageTaken(IEventHandlerArgs eventArgs)
        {
            //todo: finish BarrierEventHandler
            Debug.Log("After taking damage");
        }
    }
}

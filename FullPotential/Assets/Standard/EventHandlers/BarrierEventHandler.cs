using System;
using FullPotential.Api.Gameplay.Events;
using UnityEngine;

namespace FullPotential.Standard.EventHandlers
{
    public class BarrierEventHandler : IEventHandler
    {
        public Action<IEventHandlerArgs> BeforeEvent => HandleBeforeDamageTaken;

        public Action<IEventHandlerArgs> AfterEvent => HandleAfterDamageTaken;

        private void HandleBeforeDamageTaken(IEventHandlerArgs eventArgs)
        {
            Debug.Log("Before taking damage");
        }

        private void HandleAfterDamageTaken(IEventHandlerArgs eventArgs)
        {
            Debug.Log("After taking damage");
        }
    }
}

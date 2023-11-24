using System;
using FullPotential.Api.Gameplay.Events;
using UnityEngine;

namespace FullPotential.Standard.Accessories
{
    public class BarrierEventHandler : IEventHandler
    {
        public Action<IEventHandlerArgs> BeforeHandler => HandleBeforeDamageTaken;

        public Action<IEventHandlerArgs> AfterHandler => HandleAfterDamageTaken;

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

using System;
using FullPotential.Api.Gameplay.Events;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Accessories
{
    public class BarrierEventHandler : IEventHandler
    {
        //todo: zzz v0.4.1 - finish BarrierEventHandler
        public NetworkLocation Location => NetworkLocation.Server;

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

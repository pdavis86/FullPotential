using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry.Events
{
    internal class LivingEntityHealthChangedEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Client;

        public Action<IEventHandlerArgs> BeforeHandler => null;

        public Action<IEventHandlerArgs> AfterHandler => HandleAfterValueChanged;

        private void HandleAfterValueChanged(IEventHandlerArgs eventArgs)
        {
            var valueChangedArgs = (ResourceValueChangedEventArgs)eventArgs;

            if (valueChangedArgs.ResourceTypeId != ResourceTypeIds.HealthId)
            {
                return;
            }

            valueChangedArgs.LivingEntity.UpdateUiHealthAndDefenceValues();
        }
    }
}

using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable UnusedParameter.Global

namespace FullPotential.Api.Modding
{
    public interface IMod
    {
        IEnumerable<Type> GetRegisterableTypes();

        IEnumerable<Type> GetRegisterableVisuals();

        IEnumerable<string> GetNetworkPrefabAddresses();

        void RegisterEventHandlers(IEventManager eventManager);
    }
}

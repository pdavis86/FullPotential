using System;
using System.Collections.Generic;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Modding
{
    public interface IMod
    {
        IEnumerable<Type> GetRegisterableTypes();

        IEnumerable<string> GetNetworkPrefabAddresses();

        void RegisterEventHandlers(IEventManager eventManager);
    }
}

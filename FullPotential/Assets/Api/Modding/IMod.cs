using System;
using System.Collections.Generic;

namespace FullPotential.Api.Modding
{
    public interface IMod
    {
        IEnumerable<Type> GetRegisterableTypes();

        IEnumerable<Type> GetRegisterableVisuals();

        IEnumerable<string> GetNetworkPrefabAddresses();
    }
}

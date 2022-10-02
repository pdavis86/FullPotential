using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullPotential.Api.Registry
{
    public interface IMod
    {
        IEnumerable<Type> GetRegisterableTypes();

        IEnumerable<GameObject> GetNetworkPrefabs();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullPotential.Api.Modding
{
    public interface IMod
    {
        IEnumerable<Type> GetRegisterableTypes();

        IEnumerable<GameObject> GetNetworkPrefabs();
    }
}

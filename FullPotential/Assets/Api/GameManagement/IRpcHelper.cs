using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable UnusedParameter.Global
// ReSharper disable once UnusedMemberInSuper.Global

namespace FullPotential.Api.GameManagement
{
    public interface IRpcHelper
    {
        ClientRpcParams ForNearbyPlayers(Vector3 position);

        ClientRpcParams ForNearbyPlayersExcept(Vector3 position, ulong exceptClientId);

        ClientRpcParams ForNearbyPlayersExcept(Vector3 position, IEnumerable<ulong> exceptClientIds);
    }
}

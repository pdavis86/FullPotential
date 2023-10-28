using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace FullPotential.Api.Networking
{
    public interface IRpcService
    {
        ClientRpcParams ForPlayer(ulong clientId);

        ClientRpcParams ForNearbyPlayers(Vector3 position);

        ClientRpcParams ForNearbyPlayersExcept(Vector3 position, ulong exceptClientId);

        ClientRpcParams ForNearbyPlayersExcept(Vector3 position, IEnumerable<ulong> exceptClientIds);
    }
}

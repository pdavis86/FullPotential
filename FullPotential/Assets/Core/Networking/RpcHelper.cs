using System.Collections.Generic;
using System.Linq;
using FullPotential.Api.GameManagement;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Networking
{
    public class RpcHelper : IRpcHelper
    {
        public ClientRpcParams ForPlayer(ulong clientId)
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            };
        }

        public ClientRpcParams ForNearbyPlayers(Vector3 position)
        {
            //Debug.Log("Sending RPC call to all clients near " + position);
            return new ClientRpcParams();
        }

        public ClientRpcParams ForNearbyPlayersExcept(Vector3 position, ulong exceptClientId)
        {
            return ForNearbyPlayersExcept(position, new[] { exceptClientId });
        }

        public ClientRpcParams ForNearbyPlayersExcept(Vector3 position, IEnumerable<ulong> exceptClientIds)
        {
            //Debug.Log($"Sending RPC call to all clients except {string.Join(',', exceptClientIds)} near " + position);

            var clientIds = NetworkManager.Singleton.ConnectedClientsIds.Except(exceptClientIds);

            var clientRpcParams = new ClientRpcParams();
            clientRpcParams.Send.TargetClientIds = clientIds.ToList();
            return clientRpcParams;
        }
    }
}

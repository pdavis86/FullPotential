using Unity.Netcode;
using UnityEngine;

// ReSharper disable UnusedParameter.Global

namespace FullPotential.Api.Gameplay
{
    //todo: I don't think this is something that should be in API
    public interface IPlayerBehaviour
    {
        void ShowHealthChangeClientRpc(Vector3 position, int change, bool isCritical, ClientRpcParams clientRpcParams);
    }
}

using Unity.Netcode;

using UnityEngine;

namespace FullPotential.Api.Gameplay
{
    public interface IPlayerBehaviour
    {
        void ShowHealthChangeClientRpc(Vector3 position, int change, bool isCritical, ClientRpcParams clientRpcParams);
    }
}

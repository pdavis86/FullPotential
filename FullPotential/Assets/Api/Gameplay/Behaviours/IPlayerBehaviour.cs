using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Behaviours
{
    public interface IPlayerBehaviour
    {
        void ShowHealthChangeClientRpc(Vector3 position, int change, ClientRpcParams clientRpcParams);
    }
}

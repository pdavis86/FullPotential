using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Behaviours
{
    public interface IPlayerBehaviour
    {
        void ShowDamageClientRpc(Vector3 position, string damage, ClientRpcParams clientRpcParams);
    }
}

using Unity.Netcode;

using UnityEngine;

namespace FullPotential.Api.Gameplay
{
    public interface IMoveable
    {
        void ApplyMovementForceClientRpc(Vector3 force, ForceMode forceMode, ClientRpcParams clientRpcParams);
    }
}

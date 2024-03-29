﻿using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Api.Gameplay
{
    public interface IMoveable
    {
        // ReSharper disable once UnusedParameter.Global
        void ApplyMovementForceClientRpc(Vector3 force, ForceMode forceMode, ClientRpcParams clientRpcParams);
    }
}

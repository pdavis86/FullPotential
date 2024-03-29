﻿using Unity.Netcode;
using UnityEngine;

// ReSharper disable UnusedParameter.Global

namespace FullPotential.Api.Gameplay
{
    public interface IPlayerBehaviour
    {
        void ShowHealthChangeClientRpc(Vector3 position, int change, bool isCritical, ClientRpcParams clientRpcParams);
    }
}

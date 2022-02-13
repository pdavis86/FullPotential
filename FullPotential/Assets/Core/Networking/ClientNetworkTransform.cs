﻿using Unity.Netcode.Components;
using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Networking
{
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            CanCommitToTransform = IsOwner;
        }

        protected override void Update()
        {
            base.Update();

            if (
                NetworkManager == null
                || (!NetworkManager.IsConnectedClient && !NetworkManager.IsListening)
                || !CanCommitToTransform)
            {
                return;
            }

            TryCommitTransformToServer(transform, NetworkManager.LocalTime.Time);
        }
    }
}
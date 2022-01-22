using Unity.Netcode.Components;
using UnityEngine;

namespace FullPotential.Core.Behaviours.Networking
{
    //Netcode for GameObjects way to handle client-side transform data
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

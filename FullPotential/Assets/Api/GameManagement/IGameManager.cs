using FullPotential.Api.Scenes;
using FullPotential.Api.Ui;

using Unity.Netcode;

using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.GameManagement
{
    public interface IGameManager
    {
        DefaultInputActions InputActions { get; }

        ISceneBehaviour GetSceneBehaviour();

        IUserInterface GetUserInterface();

        void SpawnPlayerNetworkObject(Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default);

        GameObject GetLocalPlayerGameObject();
    }
}

using FullPotential.Api.GameManagement.Data;
using FullPotential.Api.Scenes;
using FullPotential.Api.Ui;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Api.GameManagement
{
    public interface IGameManager
    {
        AppOptions AppOptions { get; }

        T GetService<T>();

        ISceneBehaviour GetSceneBehaviour();

        IUserInterface GetUserInterface();

        void SpawnPlayerNetworkObject(string playerToken, Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default);

        string GetLocalPlayerToken();
    }
}

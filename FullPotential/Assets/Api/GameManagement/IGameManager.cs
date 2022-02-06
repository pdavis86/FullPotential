using FullPotential.Api.Data;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry;
using FullPotential.Api.Scenes;
using FullPotential.Api.Ui;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Api.GameManagement
{
    public interface IGameManager
    {
        IAttackHelper AttackHelper { get; }

        IRpcHelper RpcHelper { get; }

        ISceneBehaviour SceneBehaviour { get; }

        AppOptions AppOptions { get; }

        IUserInterface UserInterface { get; }

        ITypeRegistry TypeRegistry { get; }

        void SpawnPlayerNetworkObject(string playerToken, Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default);

        string GetLocalPlayerToken();
    }
}

using Unity.Netcode;
using UnityEngine;

// ReSharper disable UnusedParameter.Global

namespace FullPotential.Api.Scenes
{
    public interface ISceneBehaviour
    {
        SceneAttributes Attributes { get; }

        ISceneService GetSceneService();

        Transform GetTransform();

        SpawnPoint GetSpawnPoint();

        void HandleEnemyDeath();

        void MakeAnnouncementClientRpc(string announcement, ClientRpcParams clientRpcParams);
    }
}

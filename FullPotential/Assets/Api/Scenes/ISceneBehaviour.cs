using FullPotential.Api.Spawning;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Api.Scenes
{
    public interface ISceneBehaviour
    {
        SceneAttributes Attributes { get; }

        ISpawnService GetSpawnService();

        Transform GetTransform();

        SpawnPoint GetSpawnPoint(GameObject gameObjectToSpawn);

        void HandleEnemyDeath();

        // ReSharper disable once UnusedParameter.Global
        void MakeAnnouncementClientRpc(string announcement, ClientRpcParams clientRpcParams);
    }
}

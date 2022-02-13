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

        void MakeAnnouncementClientRpc(string announcement, ClientRpcParams clientRpcParams);
    }
}

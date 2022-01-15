using FullPotential.Api.Data;
using FullPotential.Core.Spawning;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Api.Behaviours
{
    public interface ISceneBehaviour
    {
        SceneAttributes Attributes { get; }

        SpawnService GetSpawnService();

        Transform GetTransform();

        SpawnPoint GetSpawnPoint(GameObject gameObjectToSpawn);

        void HandleEnemyDeath();

        void MakeAnnouncementClientRpc(string announcement, ClientRpcParams clientRpcParams);
    }
}

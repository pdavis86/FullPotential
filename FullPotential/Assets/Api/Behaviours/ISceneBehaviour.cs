using FullPotential.Api.Data;
using FullPotential.Core.Spawning;
using UnityEngine;

namespace FullPotential.Api.Behaviours
{
    public interface ISceneBehaviour
    {
        SceneAttributes Attributes { get; }

        void OnEnemyDeath();

        SpawnService GetSpawnService();

        Transform GetTransform();

        SpawnPoint GetSpawnPoint();
    }
}

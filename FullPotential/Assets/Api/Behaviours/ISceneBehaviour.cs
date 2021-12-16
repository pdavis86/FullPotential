using FullPotential.Core.Spawning;
using UnityEngine;

namespace FullPotential.Api.Behaviours
{
    public interface ISceneBehaviour
    {
        void OnEnemyDeath();

        SpawnService GetSpawnService();

        Transform GetTransform();
    }
}

using UnityEngine;

namespace FullPotential.Api.Scenes
{
    public interface ISceneBehaviour
    {
        SceneAttributes Attributes { get; }

        ISceneService GetSceneService();

        Transform GetTransform();

        SpawnPoint GetSpawnPoint();

        void HandleEnemyDeath();
    }
}

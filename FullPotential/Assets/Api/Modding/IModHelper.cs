using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Consumers;
using UnityEngine;

namespace FullPotential.Api.Modding
{
    public interface IModHelper
    {
        IGameManager GetGameManager();

        void SpawnShapeGameObject<T>(
            Consumer consumer,
            IFighter sourceFighter,
            Vector3 startPosition,
            Quaternion rotation)
            where T : IShapeBehaviour;
    }
}
using FullPotential.Api.Gameplay.Behaviours;
using UnityEngine;

namespace FullPotential.Api.Gameplay.Items
{
    public class DestroyStoppable : IStoppable
    {
        private readonly GameObject _gameObjectToDestroy;

        public DestroyStoppable(GameObject gameObjectToDestroy)
        {
            _gameObjectToDestroy = gameObjectToDestroy;
        }

        public void Stop()
        {
           Object.Destroy(_gameObjectToDestroy);
        }
    }
}

using FullPotential.Api.GameManagement;
using FullPotential.Core.GameManagement;

namespace FullPotential.Api.Utilities
{
    public static class ModHelper
    {
        private static IGameManager _gameManager;

        public static IGameManager GetGameManager()
        {
            return _gameManager ??= UnityEngine.GameObject.Find(nameof(GameManager)).GetComponent<IGameManager>();
        }
    }
}

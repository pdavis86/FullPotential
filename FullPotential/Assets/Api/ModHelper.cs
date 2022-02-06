using FullPotential.Api.GameManagement;

namespace FullPotential.Api
{
    public static class ModHelper
    {
        private static IGameManager _gameManager;

        public static IGameManager GetGameManager()
        {
            return _gameManager ??= UnityEngine.GameObject.Find("GameManager").GetComponent<IGameManager>();
        }
    }
}

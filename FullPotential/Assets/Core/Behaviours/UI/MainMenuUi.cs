using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using UnityEngine;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.Behaviours.Ui
{
    public class MainMenuUi : MonoBehaviour
    {
        private MainCanvasObjects _mainCanvasObjects;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _mainCanvasObjects = GameManager.Instance.MainCanvasObjects;
        }

        // ReSharper disable once UnusedMember.Global
        public void ForceRespawn()
        {
            GameManager.Instance.LocalGameDataStore.GameObject.GetComponent<PlayerState>().ForceRespawnServerRpc();
        }

        // ReSharper disable once UnusedMember.Global
        public void Disconnect()
        {
            _mainCanvasObjects.HideAllMenus();
            GameManager.Instance.Disconnect();
        }

        // ReSharper disable once UnusedMember.Global
        public void QuitGame()
        {
            GameManager.Instance.Quit();
        }

    }
}

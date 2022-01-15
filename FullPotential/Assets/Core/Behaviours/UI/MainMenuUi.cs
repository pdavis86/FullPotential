using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using UnityEngine;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

namespace FullPotential.Core.Behaviours.Ui
{
    public class MainMenuUi : MonoBehaviour
    {
        private MainCanvasObjects _mainCanvasObjects;

        private void Awake()
        {
            _mainCanvasObjects = GameManager.Instance.MainCanvasObjects;
        }

        public void ForceRespawn()
        {
            GameManager.Instance.LocalGameDataStore.GameObject.GetComponent<PlayerState>().RespawnServerRpc();
        }

        public void Disconnect()
        {
            _mainCanvasObjects.HideAllMenus();
            GameManager.Instance.Disconnect();
        }

        public void QuitGame()
        {
            GameManager.Instance.Quit();
        }

    }
}

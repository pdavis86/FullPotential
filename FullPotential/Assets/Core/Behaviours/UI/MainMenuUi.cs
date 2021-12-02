using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using Unity.Netcode;
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

        public void Disconnect()
        {
            Save();
            _mainCanvasObjects.HideAllMenus();
            GameManager.Disconnect();
        }

        public void QuitGame()
        {
            Save();
            GameManager.Quit();
        }

        private void Save()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }
            if (GameManager.Instance.DataStore.LocalPlayer != null)
            {
                GameManager.Instance.DataStore.LocalPlayer.GetComponent<PlayerState>().Save();
            }
        }

    }
}

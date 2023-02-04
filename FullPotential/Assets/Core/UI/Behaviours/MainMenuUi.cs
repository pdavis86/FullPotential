using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;
using UnityEngine;

// ReSharper disable UnusedType.Global

namespace FullPotential.Core.Ui.Behaviours
{
    public class MainMenuUi : MonoBehaviour
    {
        private UserInterface _userInterface;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _userInterface = GameManager.Instance.UserInterface;
        }

        // ReSharper disable once UnusedMember.Global
        public void ForceRespawn()
        {
            GameManager.Instance.LocalGameDataStore.PlayerGameObject.GetComponent<PlayerState>().ForceRespawnServerRpc();
        }

        // ReSharper disable once UnusedMember.Global
        public void Disconnect()
        {
            _userInterface.HideAllMenus();
            GameManager.Instance.Disconnect();
        }

        // ReSharper disable once UnusedMember.Global
        public void QuitGame()
        {
            GameManager.Instance.Quit();
        }

    }
}

using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;

using UnityEngine;


namespace FullPotential.Core.Ui.Behaviours
{
    public class MainMenuUi : MonoBehaviour
    {
        public void HandleForceRespawnAfterClick()
        {
            GetPlayerFighter().ForceRespawnServerRpc();
        }

        public void HandleDisconnectAfterClick()
        {
            GetPlayerFighter().SaveBeforeQuitServerRpc(true);
        }

        public void HandleQuitGameAfterClick()
        {
            GetPlayerFighter().SaveBeforeQuitServerRpc(false);
        }

        private PlayerFighter GetPlayerFighter()
        {
            return GameManager.Instance.LocalGameDataStore.PlayerGameObject.GetComponent<PlayerFighter>();
        }
    }
}

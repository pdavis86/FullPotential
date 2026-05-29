using Cysharp.Threading.Tasks;

using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;

using UnityEngine;

// ReSharper disable UnusedType.Global

namespace FullPotential.Core.Ui.Behaviours
{
    public class MainMenuUi : MonoBehaviour
    {
        // ReSharper disable once UnusedMember.Global
        public void HandleForceRespawnAfterClick()
        {
            GetPlayerFighter().ForceRespawnServerRpc();
        }

        // ReSharper disable once UnusedMember.Global
        public void HandleDisconnectAfterClick()
        {
            GetPlayerFighter().SaveBeforeQuitServerRpc(true);
        }

        // ReSharper disable once UnusedMember.Global
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

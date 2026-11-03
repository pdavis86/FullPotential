using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;

using UnityEngine;

namespace FullPotential.Core.Ui.Behaviours
{
    public class RespawnUi : MonoBehaviour
    {
        public void Respawn()
        {
            GameManager.Instance.LocalGameDataStore.PlayerGameObject.GetComponent<PlayerFighter>().Respawn();
        }
    }
}

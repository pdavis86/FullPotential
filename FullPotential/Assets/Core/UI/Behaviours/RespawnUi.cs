using FullPotential.Core.GameManagement;
using FullPotential.Core.Player;
using UnityEngine;

// ReSharper disable UnusedType.Global

namespace FullPotential.Core.Ui.Behaviours
{
    public class RespawnUi : MonoBehaviour
    {
        // ReSharper disable once UnusedMember.Global
        public void Respawn()
        {
            GameManager.Instance.LocalGameDataStore.PlayerGameObject.GetComponent<PlayerFighter>().Respawn();
        }
    }
}

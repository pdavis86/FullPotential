using FullPotential.Core.GameManagement;
using FullPotential.Core.PlayerBehaviours;
using UnityEngine;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.UiBehaviours
{
    public class RespawnUi : MonoBehaviour
    {
        // ReSharper disable once UnusedMember.Global
        public void Respawn()
        {
            GameManager.Instance.LocalGameDataStore.GameObject.GetComponent<PlayerState>().Respawn();
        }
    }
}

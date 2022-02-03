using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using UnityEngine;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.Behaviours.UI
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

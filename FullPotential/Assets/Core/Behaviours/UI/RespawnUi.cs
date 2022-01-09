using FullPotential.Core.Behaviours.GameManagement;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using UnityEngine;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace FullPotential.Core.Behaviours.UI
{
    public class RespawnUi : MonoBehaviour
    {
        public void Respawn()
        {
            GameManager.Instance.LocalGameDataStore.GameObject.GetComponent<PlayerState>().Respawn();
        }
    }
}

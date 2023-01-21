using UnityEngine;

namespace FullPotential.Core.GameManagement.Data
{
    public class LocalGameData
    {
        public string PlayerToken { get; set; }
        public GameObject PlayerGameObject { get; set; }
        public bool HasDisconnected { get; set; }
        public string DisconnectReason { get; set; }
    }
}

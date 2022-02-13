using UnityEngine;

namespace FullPotential.Core.GameManagement.Data
{
    public class LocalGameData
    {
        public string PlayerToken { get; set; }
        public GameObject GameObject { get; set; }
        public bool HasDisconnected { get; set; }
    }
}

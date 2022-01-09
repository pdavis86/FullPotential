using UnityEngine;

// ReSharper disable ArrangeAccessorOwnerBody

namespace FullPotential.Core.Storage
{
    public class LocalGameData
    {
        public bool IsDebugging { get; set; }
        public string PlayerToken { get; set; }
        public GameObject GameObject { get; set; }
        public bool HasDisconnected { get; set; }
    }
}

using UnityEngine;

// ReSharper disable ArrangeAccessorOwnerBody

namespace FullPotential.Core.Storage
{
    public class GameManagerData
    {
        public bool IsDebugging { get; set; }
        public string PlayerToken { get; set; }
        public GameObject LocalPlayer { get; set; }
        public bool HasDisconnected { get; set; }
    }
}

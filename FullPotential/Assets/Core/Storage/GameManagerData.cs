using UnityEngine;

// ReSharper disable ArrangeAccessorOwnerBody

namespace FullPotential.Assets.Core.Storage
{
    public class GameManagerData
    {
        public bool IsDebugging { get { return true; } }
        public string PlayerToken { get; set; }
        public GameObject LocalPlayer { get; set; }
    }
}

using UnityEngine;

// ReSharper disable ArrangeAccessorOwnerBody

namespace Assets.Scripts.GameManager.Models
{
    public class GameManagerData
    {
        public bool IsDebugging { get { return true; } }
        public string PlayerToken { get; set; }
        public GameObject LocalPlayer { get; set; }
    }
}

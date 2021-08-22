using UnityEngine;

namespace Assets.Core.Data
{
    public class GameManagerData
    {
        public bool IsDebugging { get { return true; } }
        public string PlayerToken { get; set; }
        public GameObject LocalPlayer { get; set; }
    }
}

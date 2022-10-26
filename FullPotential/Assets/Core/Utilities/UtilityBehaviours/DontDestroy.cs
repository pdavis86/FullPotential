using UnityEngine;

// ReSharper disable UnusedType.Global

namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    public class DontDestroy : MonoBehaviour
    {
        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
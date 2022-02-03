using UnityEngine;

// ReSharper disable once UnusedType.Global

namespace FullPotential.Core.Behaviours.UtilityBehaviours
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
using UnityEngine;

namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    // ReSharper disable once UnusedType.Global

    public class DebuggingActiveState : MonoBehaviour
    {
        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            Debug.Log($"{gameObject.name} has been enabled");
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDisable()
        {
            Debug.Log($"{gameObject.name} has been disabled");
        }
    }
}

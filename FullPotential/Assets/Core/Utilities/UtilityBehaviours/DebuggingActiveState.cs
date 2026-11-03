using UnityEngine;

namespace FullPotential.Core.Utilities.UtilityBehaviours
{

    public class DebuggingActiveState : MonoBehaviour
    {
        private void OnEnable()
        {
            Debug.Log($"{gameObject.name} has been enabled");
        }

        private void OnDisable()
        {
            Debug.Log($"{gameObject.name} has been disabled");
        }
    }
}

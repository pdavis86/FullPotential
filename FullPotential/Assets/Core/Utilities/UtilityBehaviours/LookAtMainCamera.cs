using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global

namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    public class LookAtMainCamera : MonoBehaviour
    {
        // ReSharper disable once UnusedMember.Local
        private void LateUpdate()
        {
            if (Camera.main == null)
            {
                return;
            }

            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}
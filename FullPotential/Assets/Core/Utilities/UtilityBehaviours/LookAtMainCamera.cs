using UnityEngine;


namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    public class LookAtMainCamera : MonoBehaviour
    {
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
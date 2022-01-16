using UnityEngine;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.Behaviours.UtilityBehaviours
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

            //transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}
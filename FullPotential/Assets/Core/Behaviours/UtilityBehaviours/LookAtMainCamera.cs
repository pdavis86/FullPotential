using UnityEngine;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global

namespace FullPotential.Core.Behaviours.UtilityBehaviours
{
    public class LookAtMainCamera : MonoBehaviour
    {
        private void LateUpdate()
        {
            //transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}
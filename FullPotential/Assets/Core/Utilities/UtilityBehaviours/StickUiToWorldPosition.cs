using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    public class StickUiToWorldPosition : MonoBehaviour
    {
        public Vector3 WorldPosition;

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            transform.position = Camera.main.WorldToScreenPoint(WorldPosition);
        }
    }
}

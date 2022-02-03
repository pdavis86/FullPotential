using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.UtilityBehaviours
{
    public class StickUiToWorldPosition : MonoBehaviour
    {
        public Vector3 WorldPosition;

        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            transform.position = Camera.main.WorldToScreenPoint(WorldPosition);
        }
    }
}

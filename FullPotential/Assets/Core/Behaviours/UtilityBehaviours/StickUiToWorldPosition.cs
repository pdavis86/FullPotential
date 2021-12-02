using UnityEngine;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.UtilityBehaviours
{
    public class StickUiToWorldPosition : MonoBehaviour
    {
        public Vector3 WorldPosition;

        private void Update()
        {
            transform.position = Camera.main.WorldToScreenPoint(WorldPosition);
        }
    }
}

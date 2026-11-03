using UnityEngine;


namespace FullPotential.Core.Utilities.UtilityBehaviours
{
    public class StickUiToWorldPosition : MonoBehaviour
    {
        public Vector3 WorldPosition;

        private void FixedUpdate()
        {
            transform.position = Camera.main.WorldToScreenPoint(WorldPosition);
        }
    }
}

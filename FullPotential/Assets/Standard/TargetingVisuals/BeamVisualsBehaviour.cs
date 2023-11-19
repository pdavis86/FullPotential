using FullPotential.Api.Registry.Targeting;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.TargetingVisuals
{
    public class BeamVisualsBehaviour : MonoBehaviour, ITargetingVisualsBehaviour
    {
        private Transform _cylinderParentTransform;
        private Transform _cylinderTransform;

        //todo: zzz v0.6 - Supplying StartPosition to beam means other clients see it in a weird position
        public Vector3 StartPosition { get; set; }

        public Vector3 StartDirection { get; set; }

        public bool IsLocalOwner { get; set; }

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _cylinderParentTransform = transform.GetChild(0);
            _cylinderTransform = _cylinderParentTransform.GetChild(0);
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            AdjustForFieldOfView();
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDestroy()
        {
            Destroy(_cylinderParentTransform.gameObject);
        }

        private void AdjustForFieldOfView()
        {
            if (IsLocalOwner)
            {
                //Adjust for FoV
                var adjustment = (Camera.main.fieldOfView - 50) * 0.0125f;
                _cylinderParentTransform.position -= StartDirection * adjustment;
            }
        }

        public void UpdateVisuals(bool isHitting, Vector3 hitPoint, Vector3 origin, Vector3 direction, float maxRange)
        {
            Vector3 targetDirection;
            float beamLength;
            if (isHitting)
            {
                targetDirection = (hitPoint - _cylinderParentTransform.position).normalized;
                beamLength = Vector3.Distance(_cylinderParentTransform.position, hitPoint);
            }
            else
            {
                var pointAtMaxDistance = origin + (direction * maxRange);
                targetDirection = (pointAtMaxDistance - _cylinderParentTransform.position).normalized;
                beamLength = maxRange;
            }

            _cylinderParentTransform.rotation = Quaternion.LookRotation(targetDirection);

            if (!Mathf.Approximately(_cylinderTransform.localScale.y * 2, beamLength))
            {
                _cylinderTransform.localScale = new Vector3(_cylinderTransform.localScale.x, beamLength / 2, _cylinderTransform.localScale.z);
                _cylinderTransform.position = _cylinderParentTransform.position + (_cylinderTransform.up * _cylinderTransform.localScale.y);
            }
        }

    }
}

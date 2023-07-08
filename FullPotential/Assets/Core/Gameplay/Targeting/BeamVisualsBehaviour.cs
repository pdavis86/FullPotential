using FullPotential.Api.Gameplay.Items;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Targeting
{
    public class BeamVisualsBehaviour : ConsumerVisualsBehaviour
    {
        private Transform _cylinderParentTransform;
        private Transform _cylinderTransform;
        private float _maxBeamLength;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _cylinderParentTransform = transform.GetChild(0);
            _cylinderTransform = _cylinderParentTransform.GetChild(0);
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            //todo: the problem is that the client does not have access to all of the data it needs
            //can we just send it the end position?

            _maxBeamLength = Consumer.GetRange();

            SetInitialPositionAndParent();
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            if (SourceFighter == null)
            {
                //They logged out or crashed
                Stop();
                Destroy(gameObject);
                return;
            }

            Vector3 targetDirection;
            float beamLength;
            if (Physics.Raycast(SourceFighter.LookTransform.position, SourceFighter.LookTransform.forward, out var hit, _maxBeamLength))
            {
                if (hit.transform.gameObject == SourceFighter.GameObject)
                {
                    Debug.LogWarning("Beam is hitting the source player!");
                    return;
                }

                targetDirection = (hit.point - _cylinderParentTransform.position).normalized;
                beamLength = Vector3.Distance(_cylinderParentTransform.position, hit.point);
            }
            else
            {
                var pointAtMaxDistance = SourceFighter.LookTransform.position + (SourceFighter.LookTransform.forward * _maxBeamLength);
                targetDirection = (pointAtMaxDistance - _cylinderParentTransform.position).normalized;
                beamLength = _maxBeamLength;
            }

            UpdateBeam(targetDirection, beamLength);
        }

        public override void Stop()
        {
            Destroy(_cylinderParentTransform.gameObject);
            Destroy(gameObject);
        }

        private void SetInitialPositionAndParent()
        {
            transform.position = StartPosition;

            //Move the back end to the middle
            _cylinderTransform.position += _cylinderTransform.up * _cylinderTransform.localScale.y;

            if (SourceFighter.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                //Parent to player head so the beam looks attached to the hand
                _cylinderParentTransform.parent = SourceFighter.LookTransform;

                //Adjust for FoV
                var adjustment = (Camera.main.fieldOfView - 50) * 0.0125f;
                _cylinderParentTransform.position -= SourceFighter.Transform.forward * adjustment;
            }
        }

        private void UpdateBeam(Vector3 targetDirection, float beamLength)
        {
            _cylinderParentTransform.rotation = Quaternion.LookRotation(targetDirection);

            if (!Mathf.Approximately(_cylinderTransform.localScale.y * 2, beamLength))
            {
                _cylinderTransform.localScale = new Vector3(_cylinderTransform.localScale.x, beamLength / 2, _cylinderTransform.localScale.z);
                _cylinderTransform.position = _cylinderParentTransform.position + (_cylinderTransform.up * _cylinderTransform.localScale.y);
            }
        }

    }
}

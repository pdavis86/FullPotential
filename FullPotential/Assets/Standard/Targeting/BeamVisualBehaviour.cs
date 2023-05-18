using FullPotential.Api.Gameplay.Items;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Targeting
{
    public class BeamVisualBehaviour : ConsumerVisualBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private float _leftRightAdjustment;
#pragma warning restore 0649

        private Transform _cylinderParentTransform;
        private Transform _cylinderTransform;
        private float _maxBeamLength;
        private bool _isLeftHand;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _cylinderParentTransform = transform.GetChild(0);
            _cylinderTransform = _cylinderParentTransform.GetChild(0);
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _maxBeamLength = Consumer.GetRange();

            //todo: _isLeftHand
            _isLeftHand = false;

            PerformGraphicsAdjustments();
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
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
                targetDirection = SourceFighter.LookTransform.forward;
                beamLength = _maxBeamLength;
            }

            UpdateBeam(targetDirection, beamLength);
        }

        // ReSharper disable once UnusedMember.Global
        public void OnDestroy()
        {
            Destroy(_cylinderParentTransform.gameObject);
        }

        public override void Stop()
        {
            Destroy(gameObject);
        }

        private void PerformGraphicsAdjustments()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                return;
            }

            _cylinderParentTransform.parent = SourceFighter.LookTransform;

            if (SourceFighter.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                //Adjust for FoV
                var adjustment = (Camera.main.fieldOfView - 50) * 0.0125f;
                _cylinderParentTransform.position -= SourceFighter.Transform.forward * adjustment;

                //Move it a little sideways
                _cylinderParentTransform.position += (_isLeftHand ? _leftRightAdjustment : -_leftRightAdjustment) * SourceFighter.Transform.right;
            }

            //Move the tip to the middle
            _cylinderTransform.position += _cylinderTransform.up * _cylinderTransform.localScale.y;

            _cylinderTransform.gameObject.SetActive(false);
        }

        private void UpdateBeam(Vector3 targetDirection, float beamLength)
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                return;
            }

            _cylinderParentTransform.rotation = Quaternion.LookRotation(targetDirection);

            if (!Mathf.Approximately(_cylinderTransform.localScale.y * 2, beamLength))
            {
                _cylinderTransform.localScale = new Vector3(_cylinderTransform.localScale.x, beamLength / 2, _cylinderTransform.localScale.z);
                _cylinderTransform.position = _cylinderParentTransform.position + (_cylinderTransform.up * _cylinderTransform.localScale.y);
            }

            if (!_cylinderTransform.gameObject.activeInHierarchy)
            {
                _cylinderTransform.gameObject.SetActive(true);
            }

            //todo: zzz v0.4.1 - draw a ball that will hit the end when GetTimeBetweenEffects is up
        }

    }
}

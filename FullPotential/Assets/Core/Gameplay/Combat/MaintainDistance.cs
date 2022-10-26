using System;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Networking;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Combat
{
    public class MaintainDistance : MonoBehaviour
    {
        // ReSharper disable UnassignedField.Global
        public IFighter SourceFighter;
        public float Distance;
        public float Duration;
        // ReSharper restore UnassignedField.Global

        private GameObject _targetPositionGameObject;
        private FixedJoint _joint;
        private Rigidbody _rb;
        private ClientNetworkTransform _cnt;
        private DateTime _startTime;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            CreateNewJoint();

            _rb = gameObject.GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.drag = 1;

            _cnt = gameObject.GetComponent<ClientNetworkTransform>();
            if (_cnt != null)
            {
                _cnt.IsServerAuthoritative = true;
            }

            _startTime = DateTime.Now;
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            if ((DateTime.Now - _startTime).TotalSeconds >= Duration)
            {
                Cleanup();
                return;
            }

            var targetPosition = SourceFighter.Transform.position + (SourceFighter.LookTransform.forward * Distance);

            //var vectorToTargetPosition = targetPosition - gameObject.transform.position;
            //var distanceToTargetPosition = vectorToTargetPosition.magnitude;
            //Debug.Log($"targetPosition: {targetPosition}, distance to position: {distanceToTargetPosition}");

            _targetPositionGameObject.transform.position = targetPosition;
        }

        private void CreateNewJoint()
        {
            _targetPositionGameObject = new GameObject("MaintainDistanceTarget", typeof(Rigidbody));
            _targetPositionGameObject.GetComponent<Rigidbody>().isKinematic = true;
            _targetPositionGameObject.transform.parent = GameManager.Instance.GetSceneBehaviour().GetTransform();
            _targetPositionGameObject.transform.position = gameObject.transform.position;

            _joint = gameObject.AddComponent<FixedJoint>();
            _joint.connectedBody = _targetPositionGameObject.GetComponent<Rigidbody>();
        }

        private void Cleanup()
        {
            _rb.useGravity = true;
            _rb.drag = 0;

            if (_cnt != null)
            {
                _cnt.IsServerAuthoritative = false;
            }

            Destroy(_joint);
            Destroy(_targetPositionGameObject);
            Destroy(this);
        }
    }
}

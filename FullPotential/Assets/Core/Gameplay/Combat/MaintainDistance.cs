using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Items.Types;
using FullPotential.Core.GameManagement;
using FullPotential.Core.Networking;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Combat
{
    public class MaintainDistance : MonoBehaviour
    {
        // ReSharper disable UnassignedField.Global
        public FighterBase SourceFighter;
        public float Distance;
        public Consumer Consumer;
        // ReSharper restore UnassignedField.Global

        private GameObject _targetPositionGameObject;
        private FixedJoint _joint;
        private ClientNetworkTransform _cnt;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            CreateNewJoint();

            _cnt = gameObject.GetComponent<ClientNetworkTransform>();
            if (_cnt != null)
            {
                _cnt.SetServerAuthoritative(true);
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            _targetPositionGameObject.transform.position = SourceFighter.Transform.position + (SourceFighter.LookTransform.forward * Distance);
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void OnJointBreak(float value)
        {
            Cleanup();
        }

        private void CreateNewJoint()
        {
            _targetPositionGameObject = new GameObject("MaintainDistanceFromSource", typeof(Rigidbody));
            _targetPositionGameObject.GetComponent<Rigidbody>().isKinematic = true;
            _targetPositionGameObject.transform.parent = GameManager.Instance.GetSceneBehaviour().GetTransform();
            _targetPositionGameObject.transform.position = gameObject.transform.position;

            _joint = gameObject.AddComponent<FixedJoint>();
            _joint.connectedBody = _targetPositionGameObject.GetComponent<Rigidbody>();
            _joint.breakForce = 10_000;
        }

        private void Cleanup()
        {
            SourceFighter.StopActiveConsumerBehaviour(Consumer);

            if (_cnt != null)
            {
                _cnt.SetServerAuthoritative(false);
            }

            Destroy(_targetPositionGameObject);
            Destroy(_joint);
            Destroy(this);
        }
    }
}

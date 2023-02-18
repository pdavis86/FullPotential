using FullPotential.Api.Gameplay.Combat;
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
        public IFighter SourceFighter;
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
                _cnt.IsServerAuthoritative = true;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            if (Consumer.ChargePercentage != 100)
            {
                Cleanup();
                return;
            }

            _targetPositionGameObject.transform.position = SourceFighter.Transform.position + (SourceFighter.LookTransform.forward * Distance);
        }

        private void CreateNewJoint()
        {
            _targetPositionGameObject = new GameObject("MaintainDistanceFromSource", typeof(Rigidbody));
            _targetPositionGameObject.GetComponent<Rigidbody>().isKinematic = true;
            _targetPositionGameObject.transform.parent = GameManager.Instance.GetSceneBehaviour().GetTransform();
            _targetPositionGameObject.transform.position = gameObject.transform.position;

            _joint = gameObject.AddComponent<FixedJoint>();
            _joint.connectedBody = _targetPositionGameObject.GetComponent<Rigidbody>();
        }

        private void Cleanup()
        {
            if (_cnt != null)
            {
                _cnt.IsServerAuthoritative = false;
            }

            Destroy(_targetPositionGameObject);
            Destroy(_joint);
            Destroy(this);
        }
    }
}

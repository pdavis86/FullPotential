using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Api.Utilities;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Spells.Behaviours
{
    public class SpellBeamBehaviour : MonoBehaviour, ISpellOrGadgetBehaviour
    {
        public SpellOrGadgetItemBase SpellOrGadget;
        public IPlayerStateBehaviour SourceStateBehaviour;
        public bool IsLeftHand;

#pragma warning disable 0649
        [SerializeField] private float _leftRightAdjustment;
#pragma warning restore CS0649

        private IAttackHelper _attackHelper;

        private GameObject _sourcePlayer;
        private Transform _cylinderParentTransform;
        private Transform _cylinderTransform;
        private RaycastHit _hit;
        private DelayedAction _applyEffectsAction;
        private DelayedAction _consumeResourceAction;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _cylinderParentTransform = transform.GetChild(0);
            _cylinderTransform = _cylinderParentTransform.GetChild(0);

            _attackHelper = ModHelper.GetGameManager().GetService<IAttackHelper>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (SpellOrGadget == null)
            {
                Debug.LogError("No SpellOrGadget has been set");
                Destroy(gameObject);
                return;
            }

            _sourcePlayer = GameObjectHelper.ClosestParentWithTag(gameObject, Tags.Player);

            if (_sourcePlayer == null)
            {
                Debug.LogError("No player found in parents");
                Destroy(gameObject);
                return;
            }

            PerformGraphicsAdjustments();

            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            //todo: attribute-based timings
            _applyEffectsAction = new DelayedAction(1f, () => ApplyEffects(_hit.transform.gameObject, _hit.point));
            _consumeResourceAction = new DelayedAction(0.2f, () => SourceStateBehaviour.ConsumeResource(SpellOrGadget, true));
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            //todo: attribute-based beam length
            const int maxBeamLength = 10;

            var playerCameraTransform = SourceStateBehaviour.CameraGameObject.transform;

            Vector3 targetDirection;
            float beamLength;
            if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out var hit, maxBeamLength))
            {
                if (hit.transform.gameObject == _sourcePlayer)
                {
                    Debug.LogWarning("Beam is hitting the source player!");
                    return;
                }

                _hit = hit;

                if (NetworkManager.Singleton.IsServer)
                {
                    var hitTarget = _applyEffectsAction.TryPerformAction();
                    SourceStateBehaviour.ConsumeResource(SpellOrGadget, !hitTarget);
                }

                targetDirection = (hit.point - _cylinderParentTransform.position).normalized;
                beamLength = Vector3.Distance(_cylinderParentTransform.position, hit.point);
            }
            else
            {
                targetDirection = playerCameraTransform.forward;
                beamLength = maxBeamLength;

                if (NetworkManager.Singleton.IsServer)
                {
                    _consumeResourceAction.TryPerformAction();
                }
            }

            UpdateBeam(targetDirection, beamLength);
        }

        // ReSharper disable once UnusedMember.Global
        public void OnDestroy()
        {
            Destroy(_cylinderParentTransform.gameObject);
        }

        // ReSharper disable once UnusedMember.Global
        public void StartCasting()
        {
            //Nothing here
        }

        public void Stop()
        {
            Destroy(gameObject);
        }

        public void ApplyEffects(GameObject target, Vector3? position)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            _attackHelper.DealDamage(_sourcePlayer, SpellOrGadget, target, position);
        }

        private void PerformGraphicsAdjustments()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                return;
            }

            _cylinderParentTransform.parent = SourceStateBehaviour.CameraGameObject.transform;

            if (SourceStateBehaviour.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                //todo: distance needs to vary with FOV

                //Move it a little sideways
                _cylinderParentTransform.position += (IsLeftHand ? _leftRightAdjustment : -_leftRightAdjustment) * _sourcePlayer.transform.right;
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
        }

    }
}

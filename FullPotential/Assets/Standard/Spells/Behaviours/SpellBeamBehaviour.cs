using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry.Spells;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Helpers;
using FullPotential.Api.Utilities;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Spells.Behaviours
{
    public class SpellBeamBehaviour : MonoBehaviour, ISpellBehaviour
    {
        public Spell Spell;
        public IPlayerStateBehaviour SourceStateBehaviour;
        public bool IsLeftHand;

#pragma warning disable 0649
        [SerializeField] private float _leftRightAdjustment;
#pragma warning restore CS0649

        private GameObject _sourcePlayer;
        private Transform _cylinderParentTransform;
        private Transform _cylinderTransform;
        private RaycastHit _hit;
        private DelayedAction _applyEffectsAction;
        private DelayedAction _consumeManaAction;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _cylinderParentTransform = transform.GetChild(0);
            _cylinderTransform = _cylinderParentTransform.GetChild(0);
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (Spell == null)
            {
                Debug.LogError("No spell has been set");
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
            _applyEffectsAction = new DelayedAction(1f, () => ApplySpellEffects(_hit.transform.gameObject, _hit.point));
            _consumeManaAction = new DelayedAction(0.2f, () => SourceStateBehaviour.SpendMana(Spell, true));
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
                    SourceStateBehaviour.SpendMana(Spell, !hitTarget);
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
                    _consumeManaAction.TryPerformAction();
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

        public void StopCasting()
        {
            Destroy(gameObject);
        }

        public void ApplySpellEffects(GameObject target, Vector3? position)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            ModHelper.GetGameManager().GetService<IAttackHelper>().DealDamage(_sourcePlayer, Spell, target, position);
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

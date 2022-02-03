using FullPotential.Api.Spells;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Combat;
using FullPotential.Core.Helpers;
using FullPotential.Core.Registry.Types;
using FullPotential.Core.Utilities;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Spells.Behaviours
{
    public class SpellBeamBehaviour : NetworkBehaviour, ISpellBehaviour
    {
        public string SpellId;
        public bool IsLeftHand;

#pragma warning disable 0649
        [SerializeField] private float _leftRightAdjustment;
#pragma warning restore CS0649

        private GameObject _sourcePlayer;
        private PlayerState _sourcePlayerState;
        private Spell _spell;
        private Transform _cylinderParentTransform;
        private Transform _cylinderTransform;
        private RaycastHit _hit;
        private DelayedAction _takeManaAction;
        private DelayedAction _applyEffectsAction;
        private bool _stopCasting;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _cylinderParentTransform = transform.GetChild(0);
            _cylinderTransform = _cylinderParentTransform.GetChild(0);
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _sourcePlayer = GameObjectHelper.ClosestParentWithTag(gameObject, Core.Constants.Tags.Player);

            if (_sourcePlayer == null)
            {
                Debug.LogError("No player found in parents");
                Destroy(gameObject);
                return;
            }

            _sourcePlayerState = _sourcePlayer.GetComponent<PlayerState>();

            PerformGraphicsAdjustments(_sourcePlayerState);

            if (!IsServer)
            {
                return;
            }

            _spell = _sourcePlayerState.Inventory.GetItemWithId<Spell>(SpellId);

            if (_spell == null)
            {
                Debug.LogError($"No spell found in player inventory with ID {SpellId}");
                Destroy(gameObject);
                return;
            }

            _takeManaAction = new DelayedAction(1f, () =>
            {
                if (!_sourcePlayerState.SpendMana(_spell))
                {
                    _sourcePlayerState.ToggleSpellBeam(IsLeftHand, _spell, Vector3.zero, Vector3.zero);
                    _stopCasting = true;
                }
            });

            _applyEffectsAction = new DelayedAction(1f, () =>
            {
                //Debug.Log($"Player {_sourcePlayer.name} is hitting {hit.transform.gameObject.name} with beam spell {_spell.Name} at distance {hit.distance}");
                ApplySpellEffects(_hit.transform.gameObject, _hit.point);
            });
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            //todo: attribute-based beam length
            const int maxBeamLength = 10;

            if (IsServer)
            {
                _takeManaAction.TryPerformAction();

                if (_stopCasting)
                {
                    return;
                }
            }

            var playerCameraTransform = _sourcePlayerState.PlayerCamera.transform;

            Vector3 targetDirection;
            float beamLength;
            if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out var hit, maxBeamLength))
            {
                if (hit.transform.gameObject == _sourcePlayer)
                {
                    Debug.LogWarning("Beam is hitting the source player!");
                    return;
                }

                //Debug.Log("Beam is hitting the object " + hit.transform.name);

                _hit = hit;

                if (IsServer)
                {
                    _applyEffectsAction.TryPerformAction();
                }

                targetDirection = (hit.point - _cylinderParentTransform.position).normalized;
                beamLength = Vector3.Distance(_cylinderParentTransform.position, hit.point);
            }
            else
            {
                targetDirection = playerCameraTransform.forward;
                beamLength = maxBeamLength;
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

        public override void OnDestroy()
        {
            base.OnDestroy();

            Destroy(_cylinderParentTransform.gameObject);
        }

        private void PerformGraphicsAdjustments(PlayerState playerState)
        {
            _cylinderParentTransform.parent = playerState.PlayerCamera.transform;

            if (playerState.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                //Move it a little sideways
                _cylinderParentTransform.position += (IsLeftHand ? _leftRightAdjustment : -_leftRightAdjustment) * _cylinderParentTransform.right;
            }

            //Move the tip to the middle
            _cylinderTransform.position += _cylinderTransform.up * _cylinderTransform.localScale.y;

            _cylinderTransform.gameObject.SetActive(false);
        }

        public void ApplySpellEffects(GameObject target, Vector3? position)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
        }

    }
}

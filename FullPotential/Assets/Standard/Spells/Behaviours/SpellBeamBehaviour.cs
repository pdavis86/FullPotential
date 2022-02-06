using FullPotential.Api;
using FullPotential.Api.Constants;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Helpers;
using FullPotential.Api.Registry.Spells;
using FullPotential.Api.Utilities;
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
        private IPlayerStateBehaviour _sourcePlayerState;
        private Spell _spell;
        private Transform _cylinderParentTransform;
        private Transform _cylinderTransform;
        private RaycastHit _hit;
        private DelayedAction _applyEffectsAction;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _cylinderParentTransform = transform.GetChild(0);
            _cylinderTransform = _cylinderParentTransform.GetChild(0);
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _sourcePlayer = GameObjectHelper.ClosestParentWithTag(gameObject, Tags.Player);

            if (_sourcePlayer == null)
            {
                Debug.LogError("No player found in parents");
                Destroy(gameObject);
                return;
            }

            _sourcePlayerState = _sourcePlayer.GetComponent<IPlayerStateBehaviour>();

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

            _applyEffectsAction = new DelayedAction(1f, () =>
            {
                ApplySpellEffects(_hit.transform.gameObject, _hit.point);
            });
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            //todo: attribute-based beam length
            const int maxBeamLength = 10;

            var playerCameraTransform = _sourcePlayerState.PlayerCameraGameObject.transform;

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

        private void PerformGraphicsAdjustments(IPlayerStateBehaviour playerState)
        {
            _cylinderParentTransform.parent = playerState.PlayerCameraGameObject.transform;

            if (playerState.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                //Move it a little sideways
                _cylinderParentTransform.position += (IsLeftHand ? _leftRightAdjustment : -_leftRightAdjustment) * _sourcePlayer.transform.right;
            }

            //Move the tip to the middle
            _cylinderTransform.position += _cylinderTransform.up * _cylinderTransform.localScale.y;

            _cylinderTransform.gameObject.SetActive(false);
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

            ModHelper.GetGameManager().AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
        }

    }
}

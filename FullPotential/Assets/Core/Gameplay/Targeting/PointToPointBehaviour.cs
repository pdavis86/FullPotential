using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Targeting;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Targeting
{
    public class PointToPointBehaviour : NetworkBehaviour, ITargetingBehaviour
    {
        private ICombatService _combatService;
        private ITypeRegistry _typeRegistry;
        private IRpcService _rpcService;

        private float _maxBeamLength;
        private RaycastHit _hit;
        private DelayedAction _applyEffectsAction;
        private GameObject _visualsGameObject;
        private ITargetingVisualsBehaviour _visualsBehaviour;

        private readonly NetworkVariable<long> _fighterClientId = new NetworkVariable<long>();
        private readonly NetworkVariable<Vector3> _startDirection = new NetworkVariable<Vector3>();
        private readonly NetworkVariable<FixedString4096Bytes> _visualsPrefabAddress = new NetworkVariable<FixedString4096Bytes>();

        public IFighter SourceFighter { get; set; }

        public Consumer Consumer { get; set; }

        public Vector3 Direction { get; set; }

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _combatService = DependenciesContext.Dependencies.GetService<ICombatService>();
            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();
            _rpcService = DependenciesContext.Dependencies.GetService<IRpcService>();

            _visualsPrefabAddress.OnValueChanged += HandleVisualsPrefabAddressValueChanged;
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (!IsServer)
            {
                return;
            }

            _applyEffectsAction = new DelayedAction(
                Consumer.GetEffectTimeBetween(),
                () => _combatService.ApplyEffects(SourceFighter, Consumer, _hit.transform.gameObject, _hit.point));

            _maxBeamLength = Consumer.GetRange();

            _fighterClientId.Value = (long)SourceFighter.OwnerClientId;

            _startDirection.Value = Direction;

            _visualsPrefabAddress.Value = (Consumer.TargetingVisuals?.PrefabAddress)
                .OrIfNullOrWhitespace(Consumer.Targeting.VisualsFallbackPrefabAddress);
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            if (!IsServer)
            {
                return;
            }

            if (SourceFighter == null)
            {
                //They logged out or crashed
                Destroy(gameObject);
                return;
            }

            if (Physics.Raycast(SourceFighter.LookTransform.position, SourceFighter.LookTransform.forward, out var hit, _maxBeamLength))
            {
                if (hit.transform.gameObject == SourceFighter.GameObject)
                {
                    Debug.LogWarning("Beam is hitting the source player!");
                    return;
                }

                _hit = hit;

                if (IsServer)
                {
                    _applyEffectsAction.TryPerformAction();
                }
            }

            var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
            UpdateVisualsClientRpc(SourceFighter.LookTransform.position, SourceFighter.LookTransform.forward, _maxBeamLength, nearbyClients);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Destroy(_visualsGameObject);
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void UpdateVisualsClientRpc(Vector3 origin, Vector3 direction, float maxRange, ClientRpcParams clientRpcParams)
        {
            _visualsBehaviour?.UpdateVisuals(origin, direction, maxRange);
        }

        private void HandleVisualsPrefabAddressValueChanged(FixedString4096Bytes previousValue, FixedString4096Bytes newValue)
        {
            if (_visualsPrefabAddress.Value.ToString().IsNullOrWhiteSpace())
            {
                Debug.LogError("Cannot spawn visuals as no prefab address was provided");
                return;
            }

            _typeRegistry.LoadAddessable(
                _visualsPrefabAddress.Value.ToString(),
                visualsPrefab =>
                {
                    _visualsGameObject = Instantiate(visualsPrefab, transform);
                    _visualsGameObject.transform.Reset();

                    var sourceFighterClientId = (ulong)_fighterClientId.Value;

                    _visualsBehaviour = _visualsGameObject.GetComponent<ITargetingVisualsBehaviour>();
                    _visualsBehaviour.StartPosition = transform.position;
                    _visualsBehaviour.StartDirection = _startDirection.Value;
                    _visualsBehaviour.IsLocalOwner = sourceFighterClientId == NetworkManager.Singleton.LocalClientId;

                    if (_visualsBehaviour.IsLocalOwner)
                    {
                        var sourceFighter = NetworkManager.LocalClient.PlayerObject.GetComponent<IFighter>();

                        //Parent to player head so the beam looks attached to the hand
                        _visualsGameObject.transform.parent = sourceFighter.LookTransform;
                    }
                });
        }
    }
}

using System.Linq;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Networking;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Targeting
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

        private readonly NetworkVariable<bool> _isLocalOwner = new NetworkVariable<bool>();
        private readonly NetworkVariable<Vector3> _startDirection = new NetworkVariable<Vector3>();
        private readonly NetworkVariable<FixedString4096Bytes> _visualsPrefabAddress = new NetworkVariable<FixedString4096Bytes>();

        public FighterBase SourceFighter { get; set; }

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
                Consumer.GetChargeUpTime(),
                ApplyEffectsOnHit);

            _maxBeamLength = Consumer.GetAdjustedRange();

            _isLocalOwner.Value = SourceFighter.OwnerClientId == NetworkManager.Singleton.LocalClientId;

            _startDirection.Value = Direction;

            _visualsPrefabAddress.Value = !string.IsNullOrWhiteSpace(Consumer.TargetingVisuals?.PrefabAddress)
                ? Consumer.TargetingVisuals.PrefabAddress
                : _typeRegistry.GetRegisteredTypes<ITargetingVisuals>()
                    .FirstOrDefault(v => v.ApplicableToTypeIdString.ToString() == PointToPoint.TypeIdString)
                    ?.PrefabAddress ?? string.Empty;
        }

        private void ApplyEffectsOnHit()
        {
            _combatService.ApplyEffects(SourceFighter, Consumer, _hit.transform.gameObject, _hit.point, 1);
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

            var anythingSolid =~ LayerMask.GetMask(Layers.NonSolid);

            if (Physics.Raycast(SourceFighter.LookTransform.position, SourceFighter.LookTransform.forward, out var hit, _maxBeamLength, anythingSolid))
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

                    var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
                    UpdateVisualsClientRpc(true, hit.point, SourceFighter.LookTransform.position, SourceFighter.LookTransform.forward, _maxBeamLength, nearbyClients);
                }
            }
            else if (IsServer)
            {
                var nearbyClients = _rpcService.ForNearbyPlayers(transform.position);
                UpdateVisualsClientRpc(false, Vector3.zero, SourceFighter.LookTransform.position, SourceFighter.LookTransform.forward, _maxBeamLength, nearbyClients);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Destroy(_visualsGameObject);
        }

        // ReSharper disable once UnusedParameter.Local
        [ClientRpc]
        private void UpdateVisualsClientRpc(bool isHitting, Vector3 hitPoint, Vector3 origin, Vector3 direction, float maxRange, ClientRpcParams clientRpcParams)
        {
            _visualsBehaviour?.UpdateVisuals(isHitting, hitPoint, origin, direction, maxRange);
        }

        private void HandleVisualsPrefabAddressValueChanged(FixedString4096Bytes previousValue, FixedString4096Bytes newValue)
        {
            if (_visualsPrefabAddress.Value.ToString().IsNullOrWhiteSpace())
            {
                Debug.LogError("Cannot spawn visuals as no prefab address was provided");
                return;
            }

            _typeRegistry.LoadAddessable<GameObject>(
                _visualsPrefabAddress.Value.ToString(),
                visualsPrefab =>
                {
                    _visualsGameObject = Instantiate(visualsPrefab, transform);
                    _visualsGameObject.transform.Reset();

                    _visualsBehaviour = _visualsGameObject.GetComponent<ITargetingVisualsBehaviour>();
                    _visualsBehaviour.StartPosition = transform.position;
                    _visualsBehaviour.StartDirection = _startDirection.Value;
                    _visualsBehaviour.IsLocalOwner = _isLocalOwner.Value;

                    if (_isLocalOwner.Value)
                    {
                        var sourceFighter = NetworkManager.LocalClient.PlayerObject.GetComponent<FighterBase>();

                        //Parent to player head so the beam looks attached to the hand
                        _visualsGameObject.transform.parent = sourceFighter.LookTransform;
                    }
                });
        }
    }
}

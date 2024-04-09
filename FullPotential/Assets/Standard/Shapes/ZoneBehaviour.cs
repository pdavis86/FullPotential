using System.Linq;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Shapes;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities.Extensions;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Standard.Shapes
{
    public class ZoneBehaviour : NetworkBehaviour, IShapeBehaviour
    {
        private const float DistanceFromGround = 1f;

        private readonly NetworkVariable<FixedString4096Bytes> _visualsPrefabAddress = new NetworkVariable<FixedString4096Bytes>();

        private ICombatService _combatService;
        private ITypeRegistry _typeRegistry;

        private float _timeSinceLastEffective;
        private float _timeBetweenEffects;

        public FighterBase SourceFighter { get; set; }

        public Consumer Consumer { get; set; }

        public Vector3 Direction { get; set; }

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _combatService = DependenciesContext.Dependencies.GetService<ICombatService>();
            _typeRegistry = DependenciesContext.Dependencies.GetService<ITypeRegistry>();

            _visualsPrefabAddress.OnValueChanged += HandleVisualsPrefabAddressValueChanged;
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (!IsServer)
            {
                return;
            }

            if (Consumer == null)
            {
                Debug.LogError("No Consumer has been set");
                Destroy(gameObject);
                return;
            }

            Invoke(nameof(DestroyGameObjectAndChildren), Consumer.GetEffectDuration());

            _timeBetweenEffects = Consumer.GetChargeUpTime();

            _visualsPrefabAddress.Value = !string.IsNullOrWhiteSpace(Consumer.ShapeVisuals?.PrefabAddress)
                ? Consumer.ShapeVisuals.PrefabAddress
                : _typeRegistry.GetRegisteredTypes<IShapeVisuals>()
                    .FirstOrDefault(v => v.ApplicableToTypeIdString.ToString() == Zone.TypeIdString)
                    ?.PrefabAddress ?? string.Empty;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnTriggerStay(Collider other)
        {
            if (!IsServer)
            {
                return;
            }

            if (_timeSinceLastEffective < _timeBetweenEffects)
            {
                _timeSinceLastEffective += Time.deltaTime;
                return;
            }

            if (!other.gameObject.CompareTagAny(Tags.Player, Tags.Enemy))
            {
                return;
            }
        }

        private void ApplyEffects(Collider other)
        {
            _timeSinceLastEffective = 0;

            var position = other.ClosestPointOnBounds(transform.position);
            var adjustedPosition = position + new Vector3(0, DistanceFromGround);

            _combatService.ApplyEffects(SourceFighter, Consumer, other.gameObject, adjustedPosition, 1);
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
                    var visualsGameObject = Instantiate(visualsPrefab, transform);
                    visualsGameObject.transform.localScale = Vector3.one;
                });
        }

        private void DestroyGameObjectAndChildren()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            Destroy(gameObject);
        }
    }
}

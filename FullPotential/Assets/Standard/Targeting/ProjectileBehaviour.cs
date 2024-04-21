using System.Linq;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Targeting;
using FullPotential.Api.Utilities.Extensions;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace FullPotential.Standard.Targeting
{
    // ReSharper disable once UnusedType.Global
    public class ProjectileBehaviour : NetworkBehaviour, ITargetingBehaviour
    {
        private ICombatService _combatService;
        private ITypeRegistry _typeRegistry;

        private bool _collisionDetected;

        public FighterBase SourceFighter { get; set; }

        public Consumer Consumer { get; set; }

        public Vector3 Direction { get; set; }

        private readonly NetworkVariable<FixedString4096Bytes> _visualsPrefabAddress = new NetworkVariable<FixedString4096Bytes>();

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

            Destroy(gameObject, 3f);

            Physics.IgnoreCollision(GetComponent<Collider>(), SourceFighter.GameObject.GetComponent<Collider>());

            var rigidBody = GetComponent<Rigidbody>();

            var shotDirection = Consumer.GetShotDirection(Direction);

            rigidBody.AddForce(20f * Consumer.GetProjectileSpeed() * shotDirection, ForceMode.VelocityChange);

            if (Consumer.Shape != null)
            {
                rigidBody.useGravity = true;
            }

            _visualsPrefabAddress.Value = !string.IsNullOrWhiteSpace(Consumer.TargetingVisuals?.PrefabAddress)
                ? Consumer.TargetingVisuals.PrefabAddress
                : _typeRegistry.GetRegisteredTypes<ITargetingVisuals>()
                    .FirstOrDefault(v => v.ApplicableToTypeIdString.ToString() == Projectile.TypeIdString)
                    ?.PrefabAddress ?? string.Empty;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer)
            {
                return;
            }

            if (other.isTrigger)
            {
                return;
            }

            if (_collisionDetected)
            {
                return;
            }

            _collisionDetected = true;

            _combatService.ApplyEffects(SourceFighter, Consumer, other.gameObject, other.ClosestPointOnBounds(transform.position));

            _combatService.SpawnShapeGameObject(SourceFighter, Consumer, other.gameObject, other.ClosestPointOnBounds(transform.position), Direction);

            Consumer.StopStoppables();

            Destroy(gameObject);
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
    }
}

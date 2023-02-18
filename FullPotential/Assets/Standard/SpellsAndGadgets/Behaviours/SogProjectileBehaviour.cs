using System;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Modding;
using FullPotential.Api.Registry.Consumers;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Standard.SpellsAndGadgets.Shapes;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpellsAndGadgets.Behaviours
{
    public class SogProjectileBehaviour : MonoBehaviour, IConsumerBehaviour
    {
        private IEffectService _effectService;
        private IModHelper _modHelper;

        public Consumer Consumer;
        public IFighter SourceFighter;
        public Vector3 ForwardDirection;

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _effectService = DependenciesContext.Dependencies.GetService<IEffectService>();
            _modHelper = DependenciesContext.Dependencies.GetService<IModHelper>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (Consumer == null)
            {
                Debug.LogError("No spell has been set");
                Destroy(gameObject);
                return;
            }

            Destroy(gameObject, 3f);

            Physics.IgnoreCollision(GetComponent<Collider>(), SourceFighter.GameObject.GetComponent<Collider>());


            var affectedByGravity = Consumer.Shape != null;

            var rigidBody = GetComponent<Rigidbody>();

            var shotDirection = Consumer.GetShotDirection(ForwardDirection);

            rigidBody.AddForce(20f * Consumer.GetProjectileSpeed() * shotDirection, ForceMode.VelocityChange);

            if (affectedByGravity)
            {
                rigidBody.useGravity = true;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger)
            {
                return;
            }

            ApplyEffects(other.gameObject, other.ClosestPointOnBounds(transform.position));

            Destroy(gameObject);
        }

        public void Stop()
        {
            //Nothing here
        }

        public void ApplyEffects(GameObject target, Vector3? position)
        {
            if (!position.HasValue)
            {
                throw new ArgumentException("Position Vector3 cannot be null for projectiles");
            }

            if (Consumer.Shape == null)
            {
                if (!NetworkManager.Singleton.IsServer)
                {
                    return;
                }

                _effectService.ApplyEffects(SourceFighter, Consumer, target, position);
            }
            else
            {
                Vector3 spawnPosition;
                if (!target.CompareTagAny(Tags.Player, Tags.Enemy))
                {
                    spawnPosition = position.Value;
                }
                else
                {
                    var pointUnderTarget = new Vector3(target.transform.position.x, -100, target.transform.position.z);
                    var feetOfTarget = target.GetComponent<Collider>().ClosestPointOnBounds(pointUnderTarget);

                    spawnPosition = Physics.Raycast(feetOfTarget, transform.up * -1, out var hit)
                        ? hit.point
                        : position.Value;
                }

                if (Consumer.Shape is Wall)
                {
                    var rotation = Quaternion.LookRotation(ForwardDirection);
                    rotation.x = 0;
                    rotation.z = 0;
                    _modHelper.SpawnShapeGameObject<SogWallBehaviour>(Consumer, SourceFighter, spawnPosition, rotation);
                }
                else if (Consumer.Shape is Zone)
                {
                    _modHelper.SpawnShapeGameObject<SogZoneBehaviour>(Consumer, SourceFighter, spawnPosition, Quaternion.identity);
                }
                else
                {
                    Debug.LogError($"Unexpected secondary effect for spell {Consumer.Id} '{Consumer.Name}'");
                }
            }
        }

    }
}

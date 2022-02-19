using System;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities;
using FullPotential.Standard.SpellsAndGadgets.Shapes;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Spells.Behaviours
{
    public class SpellProjectileBehaviour : MonoBehaviour, ISpellOrGadgetBehaviour
    {
        public SpellOrGadgetItemBase SpellOrGadget;
        public IPlayerStateBehaviour SourceStateBehaviour;
        public Vector3 ForwardDirection;

        private IAttackHelper _attackHelper;

        private Type _shapeType;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (SpellOrGadget == null)
            {
                Debug.LogError("No spell has been set");
                Destroy(gameObject);
                return;
            }

            Destroy(gameObject, 3f);

            Physics.IgnoreCollision(GetComponent<Collider>(), SourceStateBehaviour.GameObject.GetComponent<Collider>());

            _attackHelper = ModHelper.GetGameManager().GetService<IAttackHelper>();

            //todo: attribute-based cast speed
            var castSpeed = SpellOrGadget.Attributes.Speed / 50f;
            if (castSpeed < 0.5)
            {
                castSpeed = 0.5f;
            }

            var affectedByGravity = SpellOrGadget.Shape != null;

            _shapeType = SpellOrGadget.Shape?.GetType();

            var rigidBody = GetComponent<Rigidbody>();

            rigidBody.AddForce(20f * castSpeed * ForwardDirection, ForceMode.VelocityChange);

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

            if (_shapeType == null)
            {
                if (!NetworkManager.Singleton.IsServer)
                {
                    return;
                }

                _attackHelper.DealDamage(SourceStateBehaviour.GameObject, SpellOrGadget, target, position);
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

                if (_shapeType == typeof(Wall))
                {
                    var rotation = Quaternion.LookRotation(ForwardDirection);
                    rotation.x = 0;
                    rotation.z = 0;
                    SpellOrGadget.Shape.SpawnGameObject(SpellOrGadget, SourceStateBehaviour, spawnPosition, rotation);
                }
                else if (_shapeType == typeof(Zone))
                {
                    SpellOrGadget.Shape.SpawnGameObject(SpellOrGadget, SourceStateBehaviour, spawnPosition, Quaternion.identity);
                }
                else
                {
                    Debug.LogError($"Unexpected secondary effect for spell {SpellOrGadget.Id} '{SpellOrGadget.Name}'");
                }
            }

            Destroy(gameObject);
        }

    }
}

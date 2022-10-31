using System;
using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities;
using FullPotential.Standard.SpellsAndGadgets.Shapes;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpellsAndGadgets.Behaviours
{
    public class SogProjectileBehaviour : MonoBehaviour, ISpellOrGadgetBehaviour
    {
        public SpellOrGadgetItemBase SpellOrGadget;
        public IFighter SourceFighter;
        public Vector3 ForwardDirection;

        private IEffectService _effectService;

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

            Physics.IgnoreCollision(GetComponent<Collider>(), SourceFighter.GameObject.GetComponent<Collider>());

            _effectService = ModHelper.GetGameManager().GetService<IEffectService>();

            var affectedByGravity = SpellOrGadget.Shape != null;

            _shapeType = SpellOrGadget.Shape?.GetType();

            var rigidBody = GetComponent<Rigidbody>();

            //todo: take item accuracy into account

            rigidBody.AddForce(20f * SpellOrGadget.GetProjectileSpeed() * ForwardDirection, ForceMode.VelocityChange);

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

            if (_shapeType == null)
            {
                if (!NetworkManager.Singleton.IsServer)
                {
                    return;
                }

                _effectService.ApplyEffects(SourceFighter, SpellOrGadget, target, position);
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
                    SpellOrGadget.Shape.SpawnGameObject(SpellOrGadget, SourceFighter, spawnPosition, rotation);
                }
                else if (_shapeType == typeof(Zone))
                {
                    SpellOrGadget.Shape.SpawnGameObject(SpellOrGadget, SourceFighter, spawnPosition, Quaternion.identity);
                }
                else
                {
                    Debug.LogError($"Unexpected secondary effect for spell {SpellOrGadget.Id} '{SpellOrGadget.Name}'");
                }
            }
        }

    }
}

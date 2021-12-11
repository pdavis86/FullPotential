using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Constants;
using FullPotential.Core.Extensions;
using FullPotential.Core.Helpers;
using FullPotential.Core.Registry.Types;
using FullPotential.Core.Spells.Shapes;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Behaviours.SpellBehaviours
{
    public class SpellProjectileBehaviour : NetworkBehaviour, ISpellBehaviour
    {
        //todo: do these have to be network variables? Can we just use normal public variables as the values will not change
        public readonly NetworkVariable<ulong> PlayerClientId = new NetworkVariable<ulong>();
        public readonly NetworkVariable<FixedString64Bytes> SpellId = new NetworkVariable<FixedString64Bytes>();
        public readonly NetworkVariable<Vector3> SpellDirection = new NetworkVariable<Vector3>();

        private GameObject _sourcePlayer;
        private PlayerState _playerState;
        private Spell _spell;
        private Type _shapeType;

        private void Start()
        {
            if (!IsServer)
            {
                //No need to Debug.LogError(). We only want this behaviour on the server
                return;
            }

            Destroy(gameObject, 3f);

            _sourcePlayer = NetworkManager.Singleton.ConnectedClients[PlayerClientId.Value].PlayerObject.gameObject;

            Physics.IgnoreCollision(GetComponent<Collider>(), _sourcePlayer.GetComponent<Collider>());

            _playerState = _sourcePlayer.GetComponent<PlayerState>();

            _spell = _playerState.Inventory.GetItemWithId<Spell>(SpellId.Value.ToString());

            if (_spell == null)
            {
                Debug.LogError($"No spell found in player inventory with ID {SpellId.Value}");
                return;
            }

            var castSpeed = _spell.Attributes.Speed / 50f;
            if (castSpeed < 0.5)
            {
                castSpeed = 0.5f;
            }

            var affectedByGravity = _spell.Shape != null;

            _shapeType = _spell.Shape?.GetType();

            var rigidBody = GetComponent<Rigidbody>();

            rigidBody.AddForce(20f * castSpeed * SpellDirection.Value, ForceMode.VelocityChange);

            if (affectedByGravity)
            {
                rigidBody.useGravity = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer)
            {
                return;
            }

            //Debug.Log("Collided with " + other.gameObject.name);

            if (!other.gameObject.CompareTagAny(Tags.Player, Tags.Enemy, Tags.Ground))
            {
                return;
            }

            ApplySpellEffects(other.gameObject, other.ClosestPointOnBounds(transform.position));
        }

        public void ApplySpellEffects(GameObject target, Vector3? position)
        {
            if (!position.HasValue)
            {
                throw new ArgumentException("Position Vector3 cannot be null for projectiles");
            }

            if (_shapeType == null)
            {
                AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
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
                    var targetsFeet = target.GetComponent<Collider>().ClosestPointOnBounds(pointUnderTarget);

                    spawnPosition = Physics.Raycast(targetsFeet, transform.up * -1, out var hit)
                        ? hit.point
                        : position.Value;
                }

                if (_shapeType == typeof(Wall))
                {
                    var rotation = Quaternion.LookRotation(SpellDirection.Value);
                    rotation.x = 0;
                    rotation.z = 0;
                    _playerState.SpawnSpellWall(_spell, spawnPosition, rotation, PlayerClientId.Value);
                }
                else if (_shapeType == typeof(Zone))
                {
                    _playerState.SpawnSpellZone(_spell, spawnPosition, PlayerClientId.Value);
                }
                else
                {
                    Debug.LogError($"Unexpected secondary effect for spell {_spell.Id} '{_spell.Name}'");
                }
            }

            Destroy(gameObject);
        }

    }
}

using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Helpers;
using FullPotential.Core.Registry.Types;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace FullPotential.Core.Behaviours.SpellBehaviours
{
    public class SpellSelfBehaviour : NetworkBehaviour, ISpellBehaviour
    {
        const float _distanceBeforeReturning = 8f;

        //todo: do these have to be network variables? Can we just use normal public variables as the values will not change
        public readonly NetworkVariable<ulong> PlayerClientId = new NetworkVariable<ulong>();
        public readonly NetworkVariable<FixedString32Bytes> SpellId = new NetworkVariable<FixedString32Bytes>();
        public readonly NetworkVariable<Vector3> SpellDirection = new NetworkVariable<Vector3>();

        private GameObject _sourcePlayer;
        private Spell _spell;
        private float _castSpeed;
        private Rigidbody _rigidbody;
        private bool _returningToPlayer;

        private void Start()
        {
            if (!IsServer)
            {
                //No need to Debug.LogError(). We only want this behaviour on the server
                return;
            }

            _sourcePlayer = NetworkManager.Singleton.ConnectedClients[PlayerClientId.Value].PlayerObject.gameObject;

            _spell = _sourcePlayer.GetComponent<PlayerState>().Inventory.GetItemWithId<Spell>(SpellId.Value.ToString());

            if (_spell == null)
            {
                Debug.LogError($"No spell found in player inventory with ID {SpellId.Value}");
                return;
            }

            _castSpeed = _spell.Attributes.Speed / 50f;
            if (_castSpeed < 0.5)
            {
                _castSpeed = 0.5f;
            }

            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.AddForce(_castSpeed * 20f * SpellDirection.Value, ForceMode.VelocityChange);
        }

        private void FixedUpdate()
        {
            if (!IsServer)
            {
                return;
            }

            var distanceFromPlayer = Vector3.Distance(transform.position, _sourcePlayer.transform.position);

            if (!_returningToPlayer)
            {
                if (distanceFromPlayer >= _distanceBeforeReturning)
                {
                    //Debug.LogError("Far enough away, now returning");
                    _returningToPlayer = true;
                    ClearForce();
                }

                return;
            }

            if (distanceFromPlayer > 0.1f)
            {
                ClearForce();
                var playerDirection = (_sourcePlayer.transform.position - transform.position).normalized;
                _rigidbody.AddForce(_castSpeed * 20f * playerDirection, ForceMode.VelocityChange);
                return;
            }

            //Debug.LogError("Finally got back to the player!");
            ApplySpellEffects(_sourcePlayer, transform.position);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer)
            {
                return;
            }

            //Debug.Log("Collided with " + other.gameObject.name);

            if (other.gameObject != _sourcePlayer)
            {
                ApplySpellEffects(other.gameObject, other.ClosestPointOnBounds(transform.position));
            }
        }

        private void ClearForce()
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        public void ApplySpellEffects(GameObject target, Vector3? position)
        {
            AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
            Destroy(gameObject);
        }

    }
}

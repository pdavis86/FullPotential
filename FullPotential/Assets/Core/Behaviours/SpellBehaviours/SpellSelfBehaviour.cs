using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Helpers;
using FullPotential.Core.Registry.Types;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace FullPotential.Core.Behaviours.SpellBehaviours
{
    public class SpellSelfBehaviour : NetworkBehaviour, ISpellBehaviour
    {
        private const float _distanceBeforeReturning = 8f;

        public ulong PlayerClientId;
        public string SpellId;
        public Vector3 SpellDirection;

        private GameObject _sourcePlayer;
        private Spell _spell;
        private float _castSpeed;
        private Rigidbody _rigidBody;
        private bool _returningToPlayer;

        private void Start()
        {
            if (!IsServer)
            {
                //No need to Debug.LogError(). We only want this behaviour on the server
                return;
            }

            //todo: handle situations where player disconnects
            _sourcePlayer = NetworkManager.Singleton.ConnectedClients[PlayerClientId].PlayerObject.gameObject;

            _spell = _sourcePlayer.GetComponent<PlayerState>().Inventory.GetItemWithId<Spell>(SpellId);

            if (_spell == null)
            {
                Debug.LogError($"No spell found in player inventory with ID {SpellId}");
                return;
            }

            _castSpeed = _spell.Attributes.Speed / 50f;
            if (_castSpeed < 0.5)
            {
                _castSpeed = 0.5f;
            }

            _rigidBody = GetComponent<Rigidbody>();
            _rigidBody.AddForce(_castSpeed * 20f * SpellDirection, ForceMode.VelocityChange);
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

            if (distanceFromPlayer > 0.2f)
            {
                ClearForce();
                var playerDirection = (_sourcePlayer.transform.position - transform.position).normalized;
                _rigidBody.AddForce(_castSpeed * 20f * playerDirection, ForceMode.VelocityChange);
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
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
        }

        public void ApplySpellEffects(GameObject target, Vector3? position)
        {
            AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
            Destroy(gameObject);
        }

    }
}

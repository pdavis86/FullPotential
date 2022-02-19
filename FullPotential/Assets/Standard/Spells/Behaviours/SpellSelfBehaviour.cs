using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Utilities;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Spells.Behaviours
{
    public class SpellSelfBehaviour : MonoBehaviour, ISpellOrGadgetBehaviour
    {
        // ReSharper disable once InconsistentNaming
        private const float _distanceBeforeReturning = 8f;

        public SpellOrGadgetItemBase SpellOrGadget;
        public IPlayerStateBehaviour SourceStateBehaviour;
        public Vector3 ForwardDirection;

        private IAttackHelper _attackHelper;

        private float _castSpeed;
        private Rigidbody _rigidBody;
        private bool _returningToPlayer;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (SpellOrGadget == null)
            {
                Debug.LogError("No spell has been set");
                Destroy(gameObject);
                return;
            }

            _attackHelper = ModHelper.GetGameManager().GetService<IAttackHelper>();

            _castSpeed = SpellOrGadget.Attributes.Speed / 50f;
            if (_castSpeed < 0.5)
            {
                _castSpeed = 0.5f;
            }

            _rigidBody = GetComponent<Rigidbody>();
            _rigidBody.AddForce(_castSpeed * 20f * ForwardDirection, ForceMode.VelocityChange);
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            var distanceFromPlayer = Vector3.Distance(transform.position, SourceStateBehaviour.Transform.position);

            if (!_returningToPlayer)
            {
                if (distanceFromPlayer < _distanceBeforeReturning)
                {
                    return;
                }

                _returningToPlayer = true;
                ClearForce();
                return;
            }

            ClearForce();
            var playerDirection = (SourceStateBehaviour.Transform.position - transform.position).normalized;
            _rigidBody.AddForce(_castSpeed * 20f * playerDirection, ForceMode.VelocityChange);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnTriggerEnter(Collider other)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Destroy(gameObject);
                return;
            }

            ApplyEffects(other.gameObject, other.ClosestPointOnBounds(transform.position));
        }

        private void ClearForce()
        {
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
        }

        public void Stop()
        {
            //Nothing here
        }

        public void ApplyEffects(GameObject target, Vector3? position)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            _attackHelper.DealDamage(SourceStateBehaviour.GameObject, SpellOrGadget, target, position);
            Destroy(gameObject);
        }

    }
}

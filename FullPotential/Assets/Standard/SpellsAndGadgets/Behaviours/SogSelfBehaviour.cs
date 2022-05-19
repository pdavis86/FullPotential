using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Utilities;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpellsAndGadgets.Behaviours
{
    public class SogSelfBehaviour : MonoBehaviour, ISpellOrGadgetBehaviour
    {
        // ReSharper disable once InconsistentNaming
        private const float _distanceBeforeReturning = 8f;

        public SpellOrGadgetItemBase SpellOrGadget;
        public IFighter SourceFighter;
        public Vector3 ForwardDirection;

        private IEffectService _effectService;

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

            _effectService = ModHelper.GetGameManager().GetService<IEffectService>();

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
            var distanceFromPlayer = Vector3.Distance(transform.position, SourceFighter.Transform.position);

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
            var playerDirection = (SourceFighter.Transform.position - transform.position).normalized;
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

            if (other.isTrigger)
            {
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

            _effectService.ApplyEffects(SourceFighter.GameObject, SpellOrGadget, target, position);
            Destroy(gameObject);
        }

    }
}

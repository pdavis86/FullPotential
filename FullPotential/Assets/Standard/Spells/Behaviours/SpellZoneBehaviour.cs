using FullPotential.Api;
using FullPotential.Api.Constants;
using FullPotential.Api.Extensions;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry.Spells;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Spells.Behaviours
{
    public class SpellZoneBehaviour : MonoBehaviour, ISpellBehaviour
    {
        public Spell Spell;
        public IPlayerStateBehaviour SourceStateBehaviour;

        private float _timeSinceLastEffective;
        private float _timeBetweenEffects;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (Spell == null)
            {
                Debug.LogError("No spell has been set");
                Destroy(gameObject);
                return;
            }

            //todo: attribute-based object lifetime
            Destroy(gameObject, 10f);

            //todo: attribute-based cooldown
            _timeBetweenEffects = 0.5f;
            _timeSinceLastEffective = _timeBetweenEffects;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnTriggerStay(Collider other)
        {
            if (!NetworkManager.Singleton.IsServer)
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

            _timeSinceLastEffective = 0;

            ApplySpellEffects(other.gameObject, other.ClosestPointOnBounds(transform.position));
        }

        public void StopCasting()
        {
            //Nothing here
        }

        public void ApplySpellEffects(GameObject target, Vector3? position)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var adjustedPosition = position + new Vector3(0, 1);
            ModHelper.GetGameManager().AttackHelper.DealDamage(SourceStateBehaviour.GameObject, Spell, target, adjustedPosition);
        }
    }
}

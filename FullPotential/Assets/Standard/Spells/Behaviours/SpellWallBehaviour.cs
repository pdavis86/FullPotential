using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry.Spells;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Spells.Behaviours
{
    public class SpellWallBehaviour : MonoBehaviour, ISpellBehaviour
    {
        public Spell Spell;
        public IPlayerStateBehaviour SourceStateBehaviour;

        private IAttackHelper _attackHelper;

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

            _attackHelper = ModHelper.GetGameManager().GetService<IAttackHelper>();

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

            _timeSinceLastEffective = 0;

            if (!other.gameObject.CompareTagAny(Tags.Player, Tags.Enemy))
            {
                return;
            }

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

            _attackHelper.DealDamage(SourceStateBehaviour.GameObject, Spell, target, position);
        }

    }
}

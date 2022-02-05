using FullPotential.Api.Spells;
using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Combat;
using FullPotential.Core.Constants;
using FullPotential.Core.Extensions;
using FullPotential.Core.Registry.Types;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Spells.Behaviours
{
    public class SpellZoneBehaviour : NetworkBehaviour, ISpellBehaviour
    {
        public ulong PlayerClientId;
        public string SpellId;

        private GameObject _sourcePlayer;
        private Spell _spell;
        private float _timeSinceLastEffective;
        private float _timeBetweenEffects;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (!IsServer)
            {
                //No need to Debug.LogError(). We only want this behaviour on the server
                return;
            }

            //todo: attribute-based object lifetime
            Destroy(gameObject, 10f);

            _sourcePlayer = NetworkManager.Singleton.ConnectedClients[PlayerClientId].PlayerObject.gameObject;

            _spell = _sourcePlayer.GetComponent<PlayerState>().Inventory.GetItemWithId<Spell>(SpellId);

            if (_spell == null)
            {
                Debug.LogError($"No spell found in player inventory with ID {SpellId}");
                return;
            }

            _timeBetweenEffects = 0.5f;
            _timeSinceLastEffective = _timeBetweenEffects;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnTriggerStay(Collider other)
        {
            if (!IsServer)
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

        public void ApplySpellEffects(GameObject target, Vector3? position)
        {
            var adjustedPosition = position + new Vector3(0, 1);
            AttackHelper.DealDamage(_sourcePlayer, _spell, target, adjustedPosition);
        }
    }
}

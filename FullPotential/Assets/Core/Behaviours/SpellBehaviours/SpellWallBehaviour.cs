using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Constants;
using FullPotential.Core.Extensions;
using FullPotential.Core.Helpers;
using FullPotential.Core.Registry.Types;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace FullPotential.Core.Behaviours.SpellBehaviours
{
    public class SpellWallBehaviour : NetworkBehaviour, ISpellBehaviour
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

            //todo: change lifetime based on attributes
            Destroy(gameObject, 10f);

            //todo: handle situations where player disconnects
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
                //Debug.Log("You hit something not damageable");
                return;
            }

            ApplySpellEffects(other.gameObject, other.ClosestPointOnBounds(transform.position));
        }

        public void ApplySpellEffects(GameObject target, Vector3? position)
        {
            //throw new System.NotImplementedException();
            Debug.Log("Applying spell effects to " + target.name);

            AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
        }

    }
}

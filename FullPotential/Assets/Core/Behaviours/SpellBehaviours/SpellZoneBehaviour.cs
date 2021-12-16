using FullPotential.Core.Behaviours.PlayerBehaviours;
using FullPotential.Core.Constants;
using FullPotential.Core.Extensions;
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
    public class SpellZoneBehaviour : NetworkBehaviour, ISpellBehaviour
    {
        public readonly NetworkVariable<ulong> PlayerClientId = new NetworkVariable<ulong>();
        public readonly NetworkVariable<FixedString32Bytes> SpellId = new NetworkVariable<FixedString32Bytes>();

        private GameObject _sourcePlayer;
        private Spell _spell;
        private float _timeSinceLastEffective;
        private float _timeBetweenEffects;

#pragma warning disable IDE0051
        private void Start()
        {
            if (!IsServer)
            {
                //No need to Debug.LogError(). We only want this behaviour on the server
                return;
            }

            //todo: for how long does this live?
            Destroy(gameObject, 10f);

            _sourcePlayer = NetworkManager.Singleton.ConnectedClients[PlayerClientId.Value].PlayerObject.gameObject;

            _spell = _sourcePlayer.GetComponent<PlayerState>().Inventory.GetItemWithId<Spell>(SpellId.Value.ToString());

            if (_spell == null)
            {
                Debug.LogError($"No spell found in player inventory with ID {SpellId.Value}");
                return;
            }

            _timeBetweenEffects = 0.5f;
            _timeSinceLastEffective = _timeBetweenEffects;
        }

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
#pragma warning restore IDE0051

        public void ApplySpellEffects(GameObject target, Vector3? position)
        {
            //throw new System.NotImplementedException();
            Debug.Log("Applying spell effects to " + target.name);

            var adjustedPosition = position + new Vector3(0, 1);
            AttackHelper.DealDamage(_sourcePlayer, _spell, target, adjustedPosition);
        }
    }
}

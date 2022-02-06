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

            //todo: attribute-based object lifetime
            Destroy(gameObject, 10f);

            _sourcePlayer = NetworkManager.Singleton.ConnectedClients[PlayerClientId].PlayerObject.gameObject;

            _spell = _sourcePlayer.GetComponent<IPlayerStateBehaviour>().Inventory.GetItemWithId<Spell>(SpellId);

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

        public void StopCasting()
        {
            //Nothing here
        }

        public void ApplySpellEffects(GameObject target, Vector3? position)
        {
            ModHelper.GetGameManager().AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
        }

    }
}

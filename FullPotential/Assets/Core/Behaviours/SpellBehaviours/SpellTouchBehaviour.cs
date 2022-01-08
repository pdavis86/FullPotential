using FullPotential.Core.Helpers;
using FullPotential.Core.Registry.Types;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming

namespace FullPotential.Core.Behaviours.SpellBehaviours
{
    public class SpellTouchBehaviour : ISpellBehaviour
    {
        private const int _maxDistance = 3;

        private readonly Spell _spell;
        private readonly GameObject _sourcePlayer;

        public SpellTouchBehaviour(Spell activeSpell, Vector3 startPosition, Vector3 direction, ulong senderClientId)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            _spell = activeSpell;
            _sourcePlayer = NetworkManager.Singleton.ConnectedClients[senderClientId].PlayerObject.gameObject;

            if (Physics.Raycast(startPosition, direction, out var hit, maxDistance: _maxDistance))
            {
                //var distance = Vector3.Distance(startPosition, hit.transform.position);
                //Debug.Log($"Player {_sourcePlayer.name} touched {hit.transform.gameObject.name} with spell {activeSpell.Name} at distance {distance}");

                ApplySpellEffects(hit.transform.gameObject, hit.point);
            }
        }

        public void ApplySpellEffects(GameObject target, Vector3? position)
        {
            AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
        }

    }
}

using FullPotential.Api;
using FullPotential.Api.Registry.Spells;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Spells.Behaviours
{
    public class SpellTouchBehaviour : ISpellBehaviour
    {
        // ReSharper disable once InconsistentNaming
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
                ApplySpellEffects(hit.transform.gameObject, hit.point);
            }
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

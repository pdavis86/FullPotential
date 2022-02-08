using FullPotential.Api;
using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry.Spells;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Spells.Behaviours
{
    public class SpellTouchBehaviour : MonoBehaviour, ISpellBehaviour
    {
        // ReSharper disable once InconsistentNaming
        private const int _maxDistance = 3;

        public string SpellId;
        public Vector3 StartPosition;
        public Vector3 SpellDirection;
        public ulong PlayerClientId;

        private Spell _spell;
        private GameObject _sourcePlayer;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            _sourcePlayer = NetworkManager.Singleton.ConnectedClients[PlayerClientId].PlayerObject.gameObject;

            var playerState = _sourcePlayer.GetComponent<IPlayerStateBehaviour>();

            _spell = playerState.Inventory.GetItemWithId<Spell>(SpellId);

            if (Physics.Raycast(StartPosition, SpellDirection, out var hit, maxDistance: _maxDistance))
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
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            ModHelper.GetGameManager().AttackHelper.DealDamage(_sourcePlayer, _spell, target, position);
        }

    }
}

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

        public Spell Spell;
        public IPlayerStateBehaviour SourceStateBehaviour;
        public Vector3 StartPosition;
        public Vector3 SpellDirection;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (Spell == null)
            {
                Debug.LogError("No spell has been set");
                Destroy(gameObject);
                return;
            }

            if (Physics.Raycast(StartPosition, SpellDirection, out var hit, maxDistance: _maxDistance))
            {
                ApplySpellEffects(hit.transform.gameObject, hit.point);
            }

            Destroy(gameObject);
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

            ModHelper.GetGameManager().AttackHelper.DealDamage(SourceStateBehaviour.GameObject, Spell, target, position);
        }

    }
}

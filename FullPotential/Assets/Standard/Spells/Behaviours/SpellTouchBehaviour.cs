using FullPotential.Api.Gameplay;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Utilities;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.Spells.Behaviours
{
    public class SpellTouchBehaviour : MonoBehaviour, ISpellOrGadgetBehaviour
    {
        // ReSharper disable once InconsistentNaming
        private const int _maxDistance = 3;

        public SpellOrGadgetItemBase SpellOrGadget;
        public IPlayerStateBehaviour SourceStateBehaviour;
        public Vector3 StartPosition;
        public Vector3 ForwardDirection;

        private IAttackHelper _attackHelper;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (SpellOrGadget == null)
            {
                Debug.LogError("No spell has been set");
                Destroy(gameObject);
                return;
            }

            _attackHelper = ModHelper.GetGameManager().GetService<IAttackHelper>();

            if (Physics.Raycast(StartPosition, ForwardDirection, out var hit, maxDistance: _maxDistance))
            {
                ApplyEffects(hit.transform.gameObject, hit.point);
            }

            Destroy(gameObject);
        }

        public void Stop()
        {
            //Nothing here
        }

        public void ApplyEffects(GameObject target, Vector3? position)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            _attackHelper.DealDamage(SourceStateBehaviour.GameObject, SpellOrGadget, target, position);
        }

    }
}

using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Utilities;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpellsAndGadgets.Behaviours
{
    public class SogTouchBehaviour : MonoBehaviour, ISpellOrGadgetBehaviour
    {
        // ReSharper disable once InconsistentNaming
        private const int _maxDistance = 3;

        public SpellOrGadgetItemBase SpellOrGadget;
        public IFighter SourceFighter;
        public Vector3 StartPosition;
        public Vector3 ForwardDirection;

        private IEffectService _effectService;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (SpellOrGadget == null)
            {
                Debug.LogError("No spell has been set");
                Destroy(gameObject);
                return;
            }

            _effectService = ModHelper.GetGameManager().GetService<IEffectService>();

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

            _effectService.ApplyEffects(SourceFighter, SpellOrGadget, target, position);
        }

    }
}

using FullPotential.Api.Gameplay;
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
        public IPlayerStateBehaviour SourceStateBehaviour;
        public Vector3 StartPosition;
        public Vector3 ForwardDirection;

        private IEffectHelper _effectHelper;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (SpellOrGadget == null)
            {
                Debug.LogError("No spell has been set");
                Destroy(gameObject);
                return;
            }

            _effectHelper = ModHelper.GetGameManager().GetService<IEffectHelper>();

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

            _effectHelper.ApplyEffects(SourceStateBehaviour.GameObject, SpellOrGadget, target, position);
        }

    }
}

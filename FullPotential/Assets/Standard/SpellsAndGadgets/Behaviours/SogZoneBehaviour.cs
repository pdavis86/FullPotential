using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using FullPotential.Api.Utilities;
using FullPotential.Api.Utilities.Extensions;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpellsAndGadgets.Behaviours
{
    public class SogZoneBehaviour : MonoBehaviour, ISpellOrGadgetBehaviour
    {
        public SpellOrGadgetItemBase SpellOrGadget;
        public IFighter SourceFighter;

        private IEffectService _effectService;

        private float _timeSinceLastEffective;
        private float _timeBetweenEffects;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (SpellOrGadget == null)
            {
                Debug.LogError("No spell has been set");
                Destroy(gameObject);
                return;
            }

            Destroy(gameObject, SpellOrGadget.Attributes.GetShapeLifetime());

            _effectService = ModHelper.GetGameManager().GetService<IEffectService>();

            _timeBetweenEffects = SpellOrGadget.Attributes.GetTimeBetweenEffects();
            _timeSinceLastEffective = _timeBetweenEffects;
        }

        // ReSharper disable once UnusedMember.Local
        private void OnTriggerStay(Collider other)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            if (_timeSinceLastEffective < _timeBetweenEffects)
            {
                _timeSinceLastEffective += Time.deltaTime;
                return;
            }

            if (!other.gameObject.CompareTagAny(Tags.Player, Tags.Enemy))
            {
                return;
            }

            _timeSinceLastEffective = 0;

            ApplyEffects(other.gameObject, other.ClosestPointOnBounds(transform.position));
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

            //todo: replace hard-coded value
            var adjustedPosition = position + new Vector3(0, 1);
            _effectService.ApplyEffects(SourceFighter.GameObject, SpellOrGadget, target, adjustedPosition);
        }
    }
}

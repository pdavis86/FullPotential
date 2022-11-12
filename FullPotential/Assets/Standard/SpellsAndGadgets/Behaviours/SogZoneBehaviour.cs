using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.SpellsAndGadgets;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpellsAndGadgets.Behaviours
{
    public class SogZoneBehaviour : MonoBehaviour, ISpellOrGadgetBehaviour
    {
        private const float DistanceFromGround = 1f;

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

            Destroy(gameObject, SpellOrGadget.GetEffectDuration());

            _effectService = DependenciesContext.Dependencies.GetService<IEffectService>();

            _timeBetweenEffects = SpellOrGadget.GetEffectTimeBetween();
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

            var adjustedPosition = position + new Vector3(0, DistanceFromGround);
            _effectService.ApplyEffects(SourceFighter, SpellOrGadget, target, adjustedPosition);
        }
    }
}

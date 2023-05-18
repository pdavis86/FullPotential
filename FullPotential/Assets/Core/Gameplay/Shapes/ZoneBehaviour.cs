using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Shapes;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Unity.Constants;
using FullPotential.Api.Unity.Extensions;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Shapes
{
    public class ZoneBehaviour : MonoBehaviour, IShapeBehaviour
    {
        private const float DistanceFromGround = 1f;

        private ICombatService _combatService;

        private float _timeSinceLastEffective;
        private float _timeBetweenEffects;

        public IFighter SourceFighter { get; set; }

        public Consumer Consumer { get; set; }

        public Vector3 Direction { get; set; }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (Consumer == null)
            {
                Debug.LogError("No Consumer has been set");
                Destroy(gameObject);
                return;
            }

            Destroy(gameObject, Consumer.GetEffectDuration());

            _combatService = DependenciesContext.Dependencies.GetService<ICombatService>();

            _timeBetweenEffects = Consumer.GetEffectTimeBetween();
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

        private void ApplyEffects(GameObject target, Vector3? position)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var adjustedPosition = position + new Vector3(0, DistanceFromGround);
            _combatService.ApplyEffects(SourceFighter, Consumer, target, adjustedPosition);
        }
    }
}

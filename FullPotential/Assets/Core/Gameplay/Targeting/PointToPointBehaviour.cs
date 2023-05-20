using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Gameplay.Targeting;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Utilities;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Gameplay.Targeting
{
    public class PointToPointBehaviour : MonoBehaviour, ITargetingBehaviour
    {
        private RaycastHit _hit;
        private DelayedAction _applyEffectsAction;
        private float _maxBeamLength;

        private ICombatService _combatService;

        public IFighter SourceFighter { get; set; }

        public Consumer Consumer { get; set; }

        public Vector3 Direction { get; set; }

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _combatService = DependenciesContext.Dependencies.GetService<ICombatService>();
        }

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            _maxBeamLength = Consumer.GetRange();

            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            _applyEffectsAction = new DelayedAction(
                Consumer.GetEffectTimeBetween(),
                () => _combatService.ApplyEffects(SourceFighter, Consumer, _hit.transform.gameObject, _hit.point));
        }

        // ReSharper disable once UnusedMember.Local
        private void FixedUpdate()
        {
            if (Physics.Raycast(SourceFighter.LookTransform.position, SourceFighter.LookTransform.forward, out var hit, _maxBeamLength))
            {
                if (hit.transform.gameObject == SourceFighter.GameObject)
                {
                    Debug.LogWarning("Beam is hitting the source player!");
                    return;
                }

                _hit = hit;

                if (NetworkManager.Singleton.IsServer)
                {
                    _applyEffectsAction.TryPerformAction();
                }
            }
        }
    }
}

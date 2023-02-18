using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry.Consumers;
using Unity.Netcode;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpellsAndGadgets.Behaviours
{
    public class SogTouchBehaviour : MonoBehaviour, IConsumerBehaviour
    {
        private const int MaxDistance = 3;

        public Consumer Consumer;
        public IFighter SourceFighter;
        public Vector3 StartPosition;
        public Vector3 ForwardDirection;

        private IEffectService _effectService;

        // ReSharper disable once UnusedMember.Local
        private void Start()
        {
            if (Consumer == null)
            {
                Debug.LogError("No spell has been set");
                Destroy(gameObject);
                return;
            }

            _effectService = DependenciesContext.Dependencies.GetService<IEffectService>();

            if (Physics.Raycast(StartPosition, ForwardDirection, out var hit, MaxDistance))
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

            _effectService.ApplyEffects(SourceFighter, Consumer, target, position);
        }

    }
}

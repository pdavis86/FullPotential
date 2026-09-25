using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry.Events
{
    public class LivingEntityDiedEventHandler : IEventHandler<ResourceValueChangeEvent>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.Late;

        public UniTask<HandlerResult> HandleEventAsync(ResourceValueChangeEvent eventArgs)
        {
            if (eventArgs.NewValue > 0 || eventArgs.ResourceTypeId != ResourceTypeIds.HealthId)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            eventArgs.LivingEntity.HandleDeath();

            return UniTask.FromResult(new HandlerResult());
        }
    }
}

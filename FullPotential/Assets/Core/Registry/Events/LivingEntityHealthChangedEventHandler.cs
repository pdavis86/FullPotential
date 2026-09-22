using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Core.Registry.Events
{
    public class LivingEntityHealthChangedEventHandler : IEventHandler<ResourceValueChangedEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Client;

        public Timing Timing => Timing.After;

        public UniTask<HandlerResult> HandleEventAsync(ResourceValueChangedEventArgs eventArgs)
        {
            if (eventArgs.ResourceTypeId != ResourceTypeIds.HealthId)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            eventArgs.LivingEntity.UpdateUiHealthAndDefenceValues();

            return UniTask.FromResult(new HandlerResult());
        }
    }
}

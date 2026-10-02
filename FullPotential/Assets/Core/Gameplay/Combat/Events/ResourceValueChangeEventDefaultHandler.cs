using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Core.Gameplay.Combat.Events
{
    public class ResourceValueChangeEventDefaultHandler : IEventHandler<ResourceValueChangeEvent>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public UniTask<HandlerResult> HandleEventAsync(ResourceValueChangeEvent eventArgs)
        {
            eventArgs.LivingEntity.UpdateResourceValue(eventArgs.ResourceTypeId, eventArgs.NewValue, eventArgs.SourceEntityName, eventArgs.SourceItemName);
            return UniTask.FromResult(new HandlerResult());
        }
    }
}

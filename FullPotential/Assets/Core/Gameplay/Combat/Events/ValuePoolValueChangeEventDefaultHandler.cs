using Cysharp.Threading.Tasks;

using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Combat.Events;

namespace FullPotential.Core.Gameplay.Combat.Events
{
    public class ValuePoolValueChangeEventDefaultHandler : IEventHandler<ValuePoolValueChangeEvent>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public UniTask<HandlerResult> HandleEventAsync(ValuePoolValueChangeEvent eventArgs)
        {
            eventArgs.LivingEntity.UpdateValuePoolValue(eventArgs.ValuePoolTypeId, eventArgs.NewValue, eventArgs.SourceEntityName, eventArgs.SourceItemName);
            return UniTask.FromResult(new HandlerResult());
        }
    }
}

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Combat.Events;


namespace FullPotential.Core.Registry.Events
{
    public class LivingEntityDiedEventHandler : IEventHandler<ValuePoolValueChangeEvent>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.Late;

        public UniTask<HandlerResult> HandleEventAsync(ValuePoolValueChangeEvent eventArgs)
        {
            if (eventArgs.NewValue > 0 || eventArgs.ValuePoolTypeId != ValuePoolTypeIds.HealthId)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            eventArgs.LivingEntity.HandleDeath();

            return UniTask.FromResult(new HandlerResult());
        }
    }
}

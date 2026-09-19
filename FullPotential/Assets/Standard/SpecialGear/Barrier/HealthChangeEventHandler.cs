using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Logging;
using FullPotential.Standard.Resources;
using FullPotential.Standard.SpecialSlots;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class HealthChangeEventHandler : IEventHandler<ResourceValueChangedEventArgs>
    {
        public const string CustomDataKeyLastHit = "LastHit";

        private readonly IAuditor _logger;

        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.Before;

        public Func<ResourceValueChangedEventArgs, UniTask<HandlerResult>> HandlerAsync => HandleBeforeHealthChangeAsync;

        public HealthChangeEventHandler(IAuditorFactory auditorFactory)
        {
            _logger = auditorFactory.Create(this);
        }

        private UniTask<HandlerResult> HandleBeforeHealthChangeAsync(ResourceValueChangedEventArgs eventArgs)
        {
            if (eventArgs.ResourceTypeId != ResourceTypeIds.HealthId
                || eventArgs.Change >= 0
                || eventArgs.IsSelfInflicted)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            var barrier = eventArgs.LivingEntity.Inventory.GetItemInSlot<Api.Obsolete.Items.Types.SpecialGear>(BarrierSlot.TypeIdString);

            if (barrier == null)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            var barrierCharge = eventArgs.LivingEntity.GetResourceValue(BarrierChargeResource.TypeIdString);

            if (barrierCharge <= 0)
            {
                _logger.Debug("Barrier depleted. Taking full damage");
                return UniTask.FromResult(new HandlerResult());
            }

               barrier.SetCustomData(CustomDataKeyLastHit, DateTime.UtcNow.ToString("u"));

            eventArgs.LivingEntity.TriggerResourceValueUpdate(BarrierChargeResource.TypeIdString, eventArgs.Change, false);

            if (barrierCharge < Math.Abs(eventArgs.Change))
            {
                _logger.Debug("Barrier nearly depleted. Taking partial damage");
                eventArgs.Change += barrierCharge;
                return UniTask.FromResult(new HandlerResult(updatedEventArgs: eventArgs));
            }

            _logger.Debug("Barrier OK. Taking no damage");

            return UniTask.FromResult(new HandlerResult(NextAction.Cancel));
        }
    }
}

using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Logging;
using FullPotential.Standard.ValuePools;
using FullPotential.Standard.SpecialSlots;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class HealthChangeEventHandler : IEventHandler<ValuePoolValueChangeEvent>
    {
        public const string CustomDataKeyLastHit = "LastHit";

        private readonly IAuditor _logger;

        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.Early;

        public HealthChangeEventHandler(IAuditorFactory auditorFactory)
        {
            _logger = auditorFactory.Create(this);
        }

        public UniTask<HandlerResult> HandleEventAsync(ValuePoolValueChangeEvent eventArgs)
        {
            if (eventArgs.ValuePoolTypeId != ValuePoolTypeIds.HealthId
                || eventArgs.Change >= 0
                || eventArgs.SourceEntityName == eventArgs.LivingEntity.name)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            var barrier = eventArgs.LivingEntity.Inventory.GetItemInSlot<Items.SpecialGear>(BarrierSlot.TypeIdString);

            if (barrier == null)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            var barrierCharge = eventArgs.LivingEntity.GetValuePoolValue(BarrierChargeValuePool.TypeIdString);

            if (barrierCharge <= 0)
            {
                _logger.Debug("Barrier depleted. Taking full damage");
                return UniTask.FromResult(new HandlerResult());
            }

            barrier.SetCustomData(CustomDataKeyLastHit, DateTime.UtcNow.ToString("u"));

            eventArgs.LivingEntity.TriggerValuePoolValueUpdate(BarrierChargeValuePool.TypeIdString, eventArgs.Change, eventArgs.SourceEntityName, eventArgs.SourceItemName);

            if (barrierCharge < Math.Abs(eventArgs.Change))
            {
                _logger.Debug("Barrier nearly depleted. Taking partial damage");

                var updatedEventArgs = new ValuePoolValueChangeEvent(
                    eventArgs.LivingEntity,
                    eventArgs.ValuePoolTypeId,
                    eventArgs.NewValue,
                    eventArgs.Change + barrierCharge,
                    eventArgs.LivingEntity.GetValuePoolMax(eventArgs.ValuePoolTypeId),
                    eventArgs.SourceEntityName,
                    eventArgs.SourceItemName);

                return UniTask.FromResult(new HandlerResult(updatedEventArgs: updatedEventArgs));
            }

            _logger.Debug("Barrier OK. Taking no damage");

            return UniTask.FromResult(new HandlerResult(NextAction.Cancel));
        }
    }
}

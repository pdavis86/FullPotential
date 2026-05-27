using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Standard.Resources;
using FullPotential.Standard.SpecialSlots;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    [RegisterEvent(LivingEntityBase.ResourceValueChangeEventId)]
    public class HealthChangeEventHandler : IEventHandler<ResourceValueChangedEventArgs>
    {
        public const string CustomDataKeyLastHit = "LastHit";

        public NetworkLocation Location => NetworkLocation.Server;

        public Func<ResourceValueChangedEventArgs, UniTask> BeforeHandlerAsync => HandleBeforeHealthChangeAsync;

        public Func<ResourceValueChangedEventArgs, UniTask> AfterHandlerAsync => null;

        private UniTask HandleBeforeHealthChangeAsync(ResourceValueChangedEventArgs eventArgs)
        {
            if (eventArgs.ResourceTypeId != ResourceTypeIds.HealthId
                || eventArgs.Change >= 0)
            {
                return UniTask.CompletedTask;
            }

            var barrier = eventArgs.LivingEntity.Inventory.GetItemInSlot<Api.Items.Types.SpecialGear>(BarrierSlot.TypeIdString);

            if (barrier == null)
            {
                return UniTask.CompletedTask;
            }

            var barrierCharge = eventArgs.LivingEntity.GetResourceValue(BarrierChargeResource.TypeIdString);

            if (barrierCharge <= 0)
            {
                //Debug.Log("Barrier depleted. Taking full damage");
                return UniTask.CompletedTask;
            }

            barrier.SetCustomData(CustomDataKeyLastHit, DateTime.UtcNow.ToString("u"));

            eventArgs.LivingEntity.TriggerResourceValueUpdate(BarrierChargeResource.TypeIdString, eventArgs.Change);

            if (barrierCharge < Math.Abs(eventArgs.Change))
            {
                //Debug.Log("Barrier nearly depleted. Taking partial damage");
                eventArgs.Change += barrierCharge;
                return UniTask.CompletedTask;
            }

            //Debug.Log("Barrier OK. Taking no damage");
            eventArgs.IsDefaultHandlerCancelled = true;

            return UniTask.CompletedTask;
        }
    }
}

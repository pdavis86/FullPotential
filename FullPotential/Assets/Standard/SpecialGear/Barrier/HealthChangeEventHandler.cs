using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Standard.Resources;
using FullPotential.Standard.SpecialSlots;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class HealthChangeEventHandler : IEventHandler
    {
        public const string CustomDataKeyLastHit = "LastHit";

        public NetworkLocation Location => NetworkLocation.Server;

        public Func<IEventHandlerArgs, UniTask> BeforeHandlerAsync => HandleBeforeHealthChangeAsync;

        public Func<IEventHandlerArgs, UniTask> AfterHandlerAsync => null;

        private UniTask HandleBeforeHealthChangeAsync(IEventHandlerArgs eventArgs)
        {
            var resourceChangeArgs = (ResourceValueChangedEventArgs)eventArgs;

            if (resourceChangeArgs.ResourceTypeId != ResourceTypeIds.HealthId
                || resourceChangeArgs.Change >= 0)
            {
                return UniTask.CompletedTask;
            }

            var barrier = resourceChangeArgs.LivingEntity.Inventory.GetItemInSlot<Api.Items.Types.SpecialGear>(BarrierSlot.TypeIdString);

            if (barrier == null)
            {
                return UniTask.CompletedTask;
            }

            var barrierCharge = resourceChangeArgs.LivingEntity.GetResourceValue(BarrierChargeResource.TypeIdString);

            if (barrierCharge <= 0)
            {
                //Debug.Log("Barrier depleted. Taking full damage");
                return UniTask.CompletedTask;
            }

            barrier.SetCustomData(CustomDataKeyLastHit, DateTime.UtcNow.ToString("u"));

            resourceChangeArgs.LivingEntity.TriggerResourceValueUpdate(BarrierChargeResource.TypeIdString, resourceChangeArgs.Change);

            if (barrierCharge < Math.Abs(resourceChangeArgs.Change))
            {
                //Debug.Log("Barrier nearly depleted. Taking partial damage");
                resourceChangeArgs.Change += barrierCharge;
                return UniTask.CompletedTask;
            }

            //Debug.Log("Barrier OK. Taking no damage");
            eventArgs.IsDefaultHandlerCancelled = true;

            return UniTask.CompletedTask;
        }
    }
}

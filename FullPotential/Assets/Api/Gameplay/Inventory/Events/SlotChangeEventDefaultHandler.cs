using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Inventory.Events
{
    public class SlotChangeEventDefaultHandler : IEventHandler<SlotChangeEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public Func<SlotChangeEventArgs, UniTask<HandlerResult>> HandlerAsync => DefaultHandlerAsync;

        private UniTask<HandlerResult> DefaultHandlerAsync(SlotChangeEventArgs eventArgs)
        {
            eventArgs.Inventory.ApplyEquippedItemChange(eventArgs.ItemId, eventArgs.SlotId);
            return UniTask.FromResult(new HandlerResult());
        }
    }
}

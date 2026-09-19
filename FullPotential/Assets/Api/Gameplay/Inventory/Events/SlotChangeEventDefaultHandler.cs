using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Inventory.Events
{
    public class SlotChangeEventDefaultHandler : IEventHandler<SlotChangeEvent>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public Func<SlotChangeEvent, UniTask> HandlerAsync => DefaultHandler;

        private UniTask DefaultHandler(SlotChangeEvent eventArgs)
        {
            eventArgs.Inventory.ApplyEquippedItemChange(eventArgs.ItemId, eventArgs.SlotId);
            return UniTask.CompletedTask;
        }
    }
}

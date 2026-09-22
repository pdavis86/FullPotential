using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Inventory.Events
{
    public class SlotChangeEventDefaultHandler : IEventHandler<SlotChangeEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public UniTask<HandlerResult> HandleEventAsync(SlotChangeEventArgs eventArgs)
        {
            eventArgs.Inventory.ApplyEquippedItemChange(eventArgs.ItemId, eventArgs.SlotId);
            return UniTask.FromResult(new HandlerResult());
        }
    }
}

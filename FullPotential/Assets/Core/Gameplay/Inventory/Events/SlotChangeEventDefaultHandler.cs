using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.Events;

namespace FullPotential.Core.Gameplay.Inventory.Events
{
    public class SlotChangeEventDefaultHandler : IEventHandler<SlotChangeEvent>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public UniTask<HandlerResult> HandleEventAsync(SlotChangeEvent eventArgs)
        {
            eventArgs.Inventory.ApplyEquippedItemChange(eventArgs.ItemId, eventArgs.SlotId);
            return UniTask.FromResult(new HandlerResult());
        }
    }
}

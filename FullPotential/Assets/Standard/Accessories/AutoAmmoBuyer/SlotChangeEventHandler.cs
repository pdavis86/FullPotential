using System;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.EventArgs;
using FullPotential.Api.Ioc;
using FullPotential.Api.Utilities.Extensions;

namespace FullPotential.Standard.Accessories.AutoAmmoBuyer
{
    public class SlotChangeEventHandler : IEventHandler
    {
        private static readonly ReloadEventHandler ReloadHandler = new ReloadEventHandler();

        public Action<IEventHandlerArgs> BeforeHandler => null;

        public Action<IEventHandlerArgs> AfterHandler => HandleAfterSlotChange;

        private void HandleAfterSlotChange(IEventHandlerArgs eventArgs)
        {
            var slotChangeArgs = (SlotChangeEventArgs)eventArgs;

            if (!slotChangeArgs.SlotId.StartsWith(AutoAmmoBuyer.TypeIdString))
            {
                return;
            }

            var hasBuyerEquipped = !slotChangeArgs.ItemId.IsNullOrWhiteSpace();

            var eventManager = DependenciesContext.Dependencies.GetService<IEventManager>();

            eventManager.Unsubscribe(FighterBase.EventIdReload, ReloadHandler);

            if (hasBuyerEquipped)
            {
                eventManager.Subscribe(FighterBase.EventIdReload, ReloadHandler);
            }
        }
    }
}

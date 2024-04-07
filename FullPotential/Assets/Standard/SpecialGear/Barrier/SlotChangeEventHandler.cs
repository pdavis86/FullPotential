using System;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.EventArgs;
using FullPotential.Api.Modding;
using FullPotential.Api.Ui;
using FullPotential.Standard.Resources;
using FullPotential.Standard.SpecialSlots;
using Unity.Netcode;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    public class SlotChangeEventHandler : IEventHandler
    {
        private readonly IHud _hud;

        public NetworkLocation Location => NetworkLocation.Client;

        public Action<IEventHandlerArgs> BeforeHandler => null;

        public Action<IEventHandlerArgs> AfterHandler => HandleAfterSlotChange;

        public SlotChangeEventHandler(IModHelper modHelper)
        {
            _hud = modHelper.GetGameManager().GetUserInterface().HudOverlay;

            _hud.ToggleSliderBar(BarrierChargeResource.TypeIdString, false);
        }

        private void HandleAfterSlotChange(IEventHandlerArgs eventArgs)
        {
            var slotChangeArgs = (SlotChangeEventArgs)eventArgs;

            if (slotChangeArgs.SlotId != BarrierSlot.TypeIdString)
            {
                return;
            }

            var isBarrierEquipped = slotChangeArgs.Inventory.GetItemInSlot(BarrierSlot.TypeIdString) != null;

            if (slotChangeArgs.Inventory.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                _hud.ToggleSliderBar(BarrierChargeResource.TypeIdString, isBarrierEquipped);
            }

            if (NetworkManager.Singleton.IsServer && !isBarrierEquipped)
            {
                slotChangeArgs.LivingEntity.TriggerResourceValueUpdate(BarrierChargeResource.TypeIdString, 0, 0);
            }
        }
    }
}

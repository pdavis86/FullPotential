using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.Events;
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

        public Func<IEventHandlerArgs, UniTask> BeforeHandlerAsync => null;

        public Func<IEventHandlerArgs, UniTask> AfterHandlerAsync => HandleAfterSlotChangeAsync;

        public SlotChangeEventHandler(IGameManager gameManager)
        {
            _hud = gameManager.GetUserInterface().HudOverlay;

            _hud.ToggleSliderBar(BarrierChargeResource.TypeIdString, false);
        }

        private UniTask HandleAfterSlotChangeAsync(IEventHandlerArgs eventArgs)
        {
            var slotChangeArgs = (SlotChangeEventArgs)eventArgs;

            if (slotChangeArgs.SlotId != BarrierSlot.TypeIdString)
            {
                return UniTask.CompletedTask;
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

            return UniTask.CompletedTask;
        }
    }
}

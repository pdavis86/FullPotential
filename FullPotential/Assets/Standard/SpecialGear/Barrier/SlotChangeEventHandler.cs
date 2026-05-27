using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.Events;
using FullPotential.Api.Ui;
using FullPotential.Standard.Resources;
using FullPotential.Standard.SpecialSlots;

using Unity.Netcode;

// ReSharper disable once ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Barrier
{
    [RegisterEvent(InventoryBase.SlotChangeEventId)]
    public class SlotChangeEventHandler : IEventHandler<SlotChangeEventArgs>
    {
        private readonly IHud _hud;

        public NetworkLocation Location => NetworkLocation.Client;

        public Func<SlotChangeEventArgs, UniTask> BeforeHandlerAsync => null;

        public Func<SlotChangeEventArgs, UniTask> AfterHandlerAsync => HandleAfterSlotChangeAsync;

        public SlotChangeEventHandler(IGameManager gameManager)
        {
            _hud = gameManager.GetUserInterface().HudOverlay;

            _hud.ToggleSliderBar(BarrierChargeResource.TypeIdString, false);
        }

        private UniTask HandleAfterSlotChangeAsync(SlotChangeEventArgs eventArgs)
        {
            if (eventArgs.SlotId != BarrierSlot.TypeIdString)
            {
                return UniTask.CompletedTask;
            }

            var isBarrierEquipped = eventArgs.Inventory.GetItemInSlot(BarrierSlot.TypeIdString) != null;

            if (eventArgs.Inventory.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                _hud.ToggleSliderBar(BarrierChargeResource.TypeIdString, isBarrierEquipped);
            }

            if (NetworkManager.Singleton.IsServer && !isBarrierEquipped)
            {
                eventArgs.LivingEntity.TriggerResourceValueUpdate(BarrierChargeResource.TypeIdString, 0, 0);
            }

            return UniTask.CompletedTask;
        }
    }
}

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
    public class SlotChangeEventHandler : IEventHandler<SlotChangeEvent>
    {
        private readonly IGameManager _gameManager;
        private readonly IHud _hud;

        public NetworkLocation Location => NetworkLocation.Client;

        public Timing Timing => Timing.Late;

        public SlotChangeEventHandler(IGameManager gameManager)
        {
            _gameManager = gameManager;
            _hud = gameManager.GetUserInterface().HudOverlay;
        }

        public UniTask<HandlerResult> HandleEventAsync(SlotChangeEvent eventArgs)
        {
            if (eventArgs.SlotId != BarrierSlot.TypeIdString)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            var isBarrierEquipped = eventArgs.Inventory.GetItemInSlot(BarrierSlot.TypeIdString) != null;

            if (eventArgs.LivingEntity.gameObject == _gameManager.GetLocalPlayerGameObject())
            {
                _hud.ToggleSliderBar(BarrierChargeResource.TypeIdString, isBarrierEquipped);
            }

            // todo: only runs on client but checks IsServer
            if (NetworkManager.Singleton.IsServer && !isBarrierEquipped)
            {
                eventArgs.LivingEntity.TriggerResourceValueUpdate(BarrierChargeResource.TypeIdString, 0, 0, false);
            }

            return UniTask.FromResult(new HandlerResult());
        }
    }
}

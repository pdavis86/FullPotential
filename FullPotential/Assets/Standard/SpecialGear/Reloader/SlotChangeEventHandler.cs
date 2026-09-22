using Cysharp.Threading.Tasks;

using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.Events;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete.Items.Types;
using FullPotential.Api.Registry;
using FullPotential.Api.Ui;
using FullPotential.Standard.SpecialSlots;

using Unity.Netcode;

using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader
{
    public class SlotChangeEventHandler : IEventHandler<SlotChangeEventArgs>
    {
        private readonly IHud _hud;
        private GameObject _handWarningPrefab;

        public NetworkLocation Location => NetworkLocation.Client;

        public Timing Timing => Timing.After;

        public SlotChangeEventHandler(IGameManager gameManager, ITypeRegistry typeRegistry)
        {
            _hud = gameManager.GetUserInterface().HudOverlay;

            typeRegistry.LoadAddessable<GameObject>(
                "Standard/UI/Equipment/HandWarning.prefab",
                prefab => _handWarningPrefab = prefab);
        }

        public UniTask<HandlerResult> HandleEventAsync(SlotChangeEventArgs eventArgs)
        {
            if (eventArgs.Inventory.OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            if (eventArgs.SlotId is not HandSlotIds.LeftHand
                and not HandSlotIds.RightHand
                and not RangedWeaponReloaderSlot.TypeIdString)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            var reloaderEquipped = eventArgs.Inventory.GetItemInSlot(RangedWeaponReloaderSlot.TypeIdString);

            switch (eventArgs.SlotId)
            {
                case HandSlotIds.LeftHand:
                case HandSlotIds.RightHand:
                    AddOrRemoveHandIcon(eventArgs.Inventory, eventArgs.SlotId, reloaderEquipped);
                    break;

                case RangedWeaponReloaderSlot.TypeIdString:
                    AddOrRemoveHandIcon(eventArgs.Inventory, HandSlotIds.LeftHand, reloaderEquipped);
                    AddOrRemoveHandIcon(eventArgs.Inventory, HandSlotIds.RightHand, reloaderEquipped);
                    break;
            }

            return UniTask.FromResult(new HandlerResult());
        }

        private void AddOrRemoveHandIcon(InventoryBase inventory, string slotId, ItemBase reloaderEquipped)
        {
            var iconId = $"{slotId};ReloaderWarning";
            var isRangedWeapon = inventory.GetItemInSlot(slotId) is Weapon weapon && weapon.IsRanged;

            if (isRangedWeapon && reloaderEquipped == null)
            {
                _hud.AddHandIcon(iconId, slotId, _handWarningPrefab);
            }
            else
            {
                _hud.RemoveHandIcon(iconId);
            }
        }
    }
}

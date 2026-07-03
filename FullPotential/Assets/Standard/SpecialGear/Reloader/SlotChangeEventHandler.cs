using System;

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
    [SubscribeToEvent(InventoryBase.SlotChangeEventId)]
    public class SlotChangeEventHandler : IEventHandler<SlotChangeEventArgs>
    {
        private readonly IHud _hud;
        private GameObject _handWarningPrefab;

        public NetworkLocation Location => NetworkLocation.Client;

        public Func<SlotChangeEventArgs, UniTask> BeforeHandlerAsync => null;

        public Func<SlotChangeEventArgs, UniTask> AfterHandlerAsync => HandleAfterSlotChangeAsync;

        public SlotChangeEventHandler(IGameManager gameManager, ITypeRegistry typeRegistry)
        {
            _hud = gameManager.GetUserInterface().HudOverlay;

            typeRegistry.LoadAddessable<GameObject>(
                "Standard/UI/Equipment/HandWarning.prefab",
                prefab => _handWarningPrefab = prefab);
        }

        private UniTask HandleAfterSlotChangeAsync(SlotChangeEventArgs eventArgs)
        {
            if (eventArgs.Inventory.OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                return UniTask.CompletedTask;
            }

            if (eventArgs.SlotId != HandSlotIds.LeftHand
                && eventArgs.SlotId != HandSlotIds.RightHand
                && eventArgs.SlotId != RangedWeaponReloaderSlot.TypeIdString)
            {
                return UniTask.CompletedTask;
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

            return UniTask.CompletedTask;
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

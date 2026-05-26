using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.Events;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Registry;
using FullPotential.Api.Ui;
using FullPotential.Standard.SpecialSlots;

using Unity.Netcode;

using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader
{
    public class SlotChangeEventHandler : IEventHandler
    {
        private readonly IHud _hud;
        private GameObject _handWarningPrefab;

        public NetworkLocation Location => NetworkLocation.Client;

        public Func<IEventHandlerArgs, UniTask> BeforeHandlerAsync => null;

        public Func<IEventHandlerArgs, UniTask> AfterHandlerAsync => HandleAfterSlotChangeAsync;

        public SlotChangeEventHandler(IGameManager gameManager, ITypeRegistry typeRegistry)
        {
            _hud = gameManager.GetUserInterface().HudOverlay;

            typeRegistry.LoadAddessable<GameObject>(
                "Standard/UI/Equipment/HandWarning.prefab",
                prefab => _handWarningPrefab = prefab);
        }

        private UniTask HandleAfterSlotChangeAsync(IEventHandlerArgs eventArgs)
        {
            var slotChangeArgs = (SlotChangeEventArgs)eventArgs;

            if (slotChangeArgs.Inventory.OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                return UniTask.CompletedTask;
            }

            if (slotChangeArgs.SlotId != HandSlotIds.LeftHand
                && slotChangeArgs.SlotId != HandSlotIds.RightHand
                && slotChangeArgs.SlotId != RangedWeaponReloaderSlot.TypeIdString)
            {
                return UniTask.CompletedTask;
            }

            var reloaderEquipped = slotChangeArgs.Inventory.GetItemInSlot(RangedWeaponReloaderSlot.TypeIdString);

            switch (slotChangeArgs.SlotId)
            {
                case HandSlotIds.LeftHand:
                case HandSlotIds.RightHand:
                    AddOrRemoveHandIcon(slotChangeArgs.Inventory, slotChangeArgs.SlotId, reloaderEquipped);
                    return UniTask.CompletedTask;

                case RangedWeaponReloaderSlot.TypeIdString:
                    AddOrRemoveHandIcon(slotChangeArgs.Inventory, HandSlotIds.LeftHand, reloaderEquipped);
                    AddOrRemoveHandIcon(slotChangeArgs.Inventory, HandSlotIds.RightHand, reloaderEquipped);
                    return UniTask.CompletedTask;
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

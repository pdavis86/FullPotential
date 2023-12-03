using System;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.EventArgs;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Modding;
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

        public Action<IEventHandlerArgs> BeforeHandler => null;

        public Action<IEventHandlerArgs> AfterHandler => HandleAfterSlotChange;

        public SlotChangeEventHandler(IModHelper modHelper, ITypeRegistry typeRegistry)
        {
            _hud = modHelper.GetGameManager().GetUserInterface().HudOverlay;

            typeRegistry.LoadAddessable<GameObject>(
                "Standard/UI/Equipment/HandWarning.prefab",
                prefab => _handWarningPrefab = prefab);
        }

        private void HandleAfterSlotChange(IEventHandlerArgs eventArgs)
        {
            var slotChangeArgs = (SlotChangeEventArgs)eventArgs;

            if (slotChangeArgs.Inventory.OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                return;
            }

            if (slotChangeArgs.SlotId != HandSlotIds.LeftHand
                && slotChangeArgs.SlotId != HandSlotIds.RightHand
                && slotChangeArgs.SlotId != RangedWeaponReloaderSlot.TypeIdString)
            {
                return;
            }

            var reloaderEquipped = slotChangeArgs.Inventory.GetItemInSlot(RangedWeaponReloaderSlot.TypeIdString);

            switch (slotChangeArgs.SlotId)
            {
                case HandSlotIds.LeftHand:
                case HandSlotIds.RightHand:
                    var isLeftHand = slotChangeArgs.SlotId == HandSlotIds.LeftHand;
                    AddOrRemoveHandIcon(slotChangeArgs.Inventory, slotChangeArgs.SlotId, reloaderEquipped, isLeftHand);
                    return;

                case RangedWeaponReloaderSlot.TypeIdString:
                    AddOrRemoveHandIcon(slotChangeArgs.Inventory, HandSlotIds.LeftHand, reloaderEquipped, true);
                    AddOrRemoveHandIcon(slotChangeArgs.Inventory, HandSlotIds.RightHand, reloaderEquipped, false);
                    return;
            }
        }

        private void AddOrRemoveHandIcon(InventoryBase inventory, string slotId, ItemBase reloaderEquipped, bool isLeftHand)
        {
            var iconId = $"{slotId};ReloaderWarning";
            var isRangedWeapon = inventory.GetItemInSlot(slotId) is Weapon weapon && weapon.IsRanged;

            if (isRangedWeapon && reloaderEquipped == null)
            {
                _hud.AddHandIcon(iconId, isLeftHand, _handWarningPrefab);
            }
            else
            {
                _hud.RemoveHandIcon(iconId);
            }
        }
    }
}

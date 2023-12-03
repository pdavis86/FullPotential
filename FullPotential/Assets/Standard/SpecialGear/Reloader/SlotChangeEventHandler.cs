using System;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.EventArgs;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Modding;
using FullPotential.Api.Ui;
using FullPotential.Standard.SpecialSlots;
using Unity.Netcode;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader
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
            var hasReloaderEquipped = reloaderEquipped != null;

            switch (slotChangeArgs.SlotId)
            {
                case HandSlotIds.LeftHand:
                case HandSlotIds.RightHand:
                    var isLeftHand = slotChangeArgs.SlotId == HandSlotIds.LeftHand;
                    _hud.SetHandWarning(isLeftHand, GetIsActive(slotChangeArgs.Inventory, slotChangeArgs.SlotId, hasReloaderEquipped));
                    return;

                case RangedWeaponReloaderSlot.TypeIdString:
                    _hud.SetHandWarning(true, GetIsActive(slotChangeArgs.Inventory, HandSlotIds.LeftHand, hasReloaderEquipped));
                    _hud.SetHandWarning(false, GetIsActive(slotChangeArgs.Inventory, HandSlotIds.RightHand, hasReloaderEquipped));
                    return;
            }
        }

        private static bool GetIsActive(InventoryBase inventory, string slotId, bool hasReloaderEquipped)
        {
            var isRangedWeapon = inventory.GetItemInSlot(slotId) is Weapon weapon && weapon.IsRanged;
            return isRangedWeapon && !hasReloaderEquipped;
        }
    }
}

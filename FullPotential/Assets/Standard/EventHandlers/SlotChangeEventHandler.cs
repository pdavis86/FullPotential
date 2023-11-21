using System;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Gameplay.Inventory.EventArgs;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Modding;
using FullPotential.Api.Ui;
using FullPotential.Standard.SpecialSlots;

namespace FullPotential.Standard.EventHandlers
{
    public class SlotChangeEventHandler : IEventHandler
    {
        public Action<IEventHandlerArgs> BeforeHandler => null;

        public Action<IEventHandlerArgs> AfterHandler => HandleAfterSlotChange;

        private void HandleAfterSlotChange(IEventHandlerArgs eventArgs)
        {
            var slotChangeArgs = (SlotChangeEventArgs)eventArgs;

            if (slotChangeArgs.SlotId != HandSlotIds.LeftHand
                && slotChangeArgs.SlotId != HandSlotIds.RightHand
                && slotChangeArgs.SlotId != RangedWeaponReloader.TypeIdString)
            {
                return;
            }

            var modHelper = DependenciesContext.Dependencies.GetService<IModHelper>();
            var hud = modHelper.GetGameManager().GetUserInterface().HudOverlay;
            var hasReloaderEquipped = slotChangeArgs.Inventory.HasTypeEquipped(RangedWeaponReloader.TypeIdString);

            switch (slotChangeArgs.SlotId)
            {
                case HandSlotIds.LeftHand:
                case HandSlotIds.RightHand:
                    var isLeftHand = slotChangeArgs.SlotId == HandSlotIds.LeftHand;
                    hud.SetHandWarning(isLeftHand, GetIsActive(slotChangeArgs.Inventory, slotChangeArgs.SlotId, hasReloaderEquipped));
                    return;

                case RangedWeaponReloader.TypeIdString:
                    hud.SetHandWarning(true, GetIsActive(slotChangeArgs.Inventory, HandSlotIds.LeftHand, hasReloaderEquipped));
                    hud.SetHandWarning(false, GetIsActive(slotChangeArgs.Inventory, HandSlotIds.RightHand, hasReloaderEquipped));
                    return;
            }
        }

        private static bool GetIsActive(IInventory inventory, string slotId, bool hasReloaderEquipped)
        {
            var isRangedWeapon = inventory.GetItemInSlot(slotId) is Weapon weapon && weapon.IsRanged;
            return isRangedWeapon && !hasReloaderEquipped;
        }
    }
}

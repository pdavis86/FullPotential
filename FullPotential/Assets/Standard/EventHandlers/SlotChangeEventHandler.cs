using System;
using FullPotential.Api.Gameplay.Events;
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

            if (slotChangeArgs.SlotId != HandSlotIds.LeftHand && slotChangeArgs.SlotId != HandSlotIds.RightHand)
            {
                return;
            }

            var itemInSlot = slotChangeArgs.Inventory.GetItemInSlot(slotChangeArgs.SlotId);
            var hasReloaderEquipped = slotChangeArgs.Inventory.HasTypeEquipped(RangedWeaponReloader.TypeIdString);

            var isActive = itemInSlot != null
                           && itemInSlot is Weapon weapon
                           && weapon.IsRanged
                           && !hasReloaderEquipped;

            var modHelper = DependenciesContext.Dependencies.GetService<IModHelper>();
            var hud = modHelper.GetGameManager().GetUserInterface().HudOverlay;
            var isLeftHand = slotChangeArgs.SlotId == HandSlotIds.LeftHand;

            hud.SetHandWarning(isLeftHand, isActive);
        }
    }
}

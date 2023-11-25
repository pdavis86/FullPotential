using System;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Inventory.EventArgs;
using FullPotential.Api.Ioc;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Modding;
using FullPotential.Api.Ui;
using FullPotential.Standard.SpecialSlots;

namespace FullPotential.Standard.SpecialGear.Reloader
{
    public class SlotChangeEventHandler : IEventHandler
    {
        private static readonly ConsolidatorReloader.ReloadEventHandler ConsolidatorReloadHandler = new ConsolidatorReloader.ReloadEventHandler();
        private static readonly TeleportReloader.ReloadEventHandler TeleportReloadHandler = new TeleportReloader.ReloadEventHandler();
        private static readonly TeleportReloader.ShotFiredEventHandler TeleportShotHandler = new TeleportReloader.ShotFiredEventHandler();

        public Action<IEventHandlerArgs> BeforeHandler => null;

        public Action<IEventHandlerArgs> AfterHandler => HandleAfterSlotChange;

        private void HandleAfterSlotChange(IEventHandlerArgs eventArgs)
        {
            var slotChangeArgs = (SlotChangeEventArgs)eventArgs;

            if (slotChangeArgs.SlotId != HandSlotIds.LeftHand
                && slotChangeArgs.SlotId != HandSlotIds.RightHand
                && slotChangeArgs.SlotId != RangedWeaponReloaderSlot.TypeIdString)
            {
                return;
            }

            var modHelper = DependenciesContext.Dependencies.GetService<IModHelper>();
            var hud = modHelper.GetGameManager().GetUserInterface().HudOverlay;

            var reloaderEquipped = slotChangeArgs.Inventory.GetItemInSlot(RangedWeaponReloaderSlot.TypeIdString);
            var hasReloaderEquipped = reloaderEquipped != null;

            switch (slotChangeArgs.SlotId)
            {
                case HandSlotIds.LeftHand:
                case HandSlotIds.RightHand:
                    var isLeftHand = slotChangeArgs.SlotId == HandSlotIds.LeftHand;
                    hud.SetHandWarning(isLeftHand, GetIsActive(slotChangeArgs.Inventory, slotChangeArgs.SlotId, hasReloaderEquipped));
                    return;

                case RangedWeaponReloaderSlot.TypeIdString:
                    hud.SetHandWarning(true, GetIsActive(slotChangeArgs.Inventory, HandSlotIds.LeftHand, hasReloaderEquipped));
                    hud.SetHandWarning(false, GetIsActive(slotChangeArgs.Inventory, HandSlotIds.RightHand, hasReloaderEquipped));

                    var eventManager = DependenciesContext.Dependencies.GetService<IEventManager>();

                    eventManager.Unsubscribe(FighterBase.EventIdReload, ConsolidatorReloadHandler);
                    eventManager.Unsubscribe(FighterBase.EventIdReload, TeleportReloadHandler);
                    eventManager.Unsubscribe(FighterBase.EventIdShotFired, TeleportShotHandler);

                    if (hasReloaderEquipped)
                    {
                        if (reloaderEquipped.RegistryTypeId == ConsolidatorReloader.ConsolidatorReloader.TypeIdString)
                        {
                            eventManager.Subscribe(FighterBase.EventIdReload, ConsolidatorReloadHandler);
                        }
                        else
                        {
                            eventManager.Subscribe(FighterBase.EventIdReload, TeleportReloadHandler);
                            eventManager.Subscribe(FighterBase.EventIdShotFired, TeleportShotHandler);
                        }
                    }

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

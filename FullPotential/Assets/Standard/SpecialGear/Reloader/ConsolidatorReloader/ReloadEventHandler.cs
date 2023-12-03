using System;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Ui;
using Unity.Netcode;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader.ConsolidatorReloader
{
    public class ReloadEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Action<IEventHandlerArgs> BeforeHandler => HandleReloadBefore;

        public Action<IEventHandlerArgs> AfterHandler => null;

        private void HandleReloadBefore(IEventHandlerArgs eventArgs)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var reloadEventArgs = (ReloadEventArgs)eventArgs;

            var reloader = (Api.Items.Types.SpecialGear)reloadEventArgs.Fighter.Inventory.GetItemInSlot(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryTypeId != ConsolidatorReloader.TypeIdString)
            {
                return;
            }

            if (!reloadEventArgs.Fighter.ConsumeResource(reloader))
            {
                return;
            }

            eventArgs.IsDefaultHandlerCancelled = true;

            var fighter = reloadEventArgs.Fighter;
            
            var slotId = reloadEventArgs.IsLeftHand ? HandSlotIds.LeftHand : HandSlotIds.RightHand;
            var equippedWeapon = (Weapon)fighter.Inventory.GetItemInSlot(slotId);

            var ammoNeeded = equippedWeapon.GetAmmoMax() - equippedWeapon.Ammo;

            FighterBase.ReloadAndUpdateClientInventory(reloadEventArgs, ammoNeeded);
        }
    }
}

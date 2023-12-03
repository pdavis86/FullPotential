using System;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Items.Types;
using FullPotential.Api.Ui;
using Unity.Netcode;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader.TeleportReloader
{
    public class ShotFiredEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Action<IEventHandlerArgs> BeforeHandler => null;

        public Action<IEventHandlerArgs> AfterHandler => HandleShotFired;

        private void HandleShotFired(IEventHandlerArgs eventArgs)
        {
            var shotFiredEventArgs = (ShotFiredEventArgs) eventArgs;

            var reloader = (Api.Items.Types.SpecialGear)shotFiredEventArgs.Fighter.Inventory.GetItemInSlot(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryTypeId != TeleportReloader.TypeIdString)
            {
                return;
            }

            if (!shotFiredEventArgs.Fighter.ConsumeResource(reloader, true, !NetworkManager.Singleton.IsServer))
            {
                return;
            }

            var fighter = shotFiredEventArgs.Fighter;

            var slotId = shotFiredEventArgs.IsLeftHand ? HandSlotIds.LeftHand : HandSlotIds.RightHand;
            var equippedWeapon = (Weapon)fighter.Inventory.GetItemInSlot(slotId);

            var ammoMax = equippedWeapon.GetAmmoMax();
            var ammoNeeded = ammoMax - equippedWeapon.Ammo;
            
            var reloadEventArgs = new ReloadEventArgs(fighter, shotFiredEventArgs.IsLeftHand);

            FighterBase.ReloadAndUpdateClientInventory(reloadEventArgs, ammoNeeded);
        }
    }
}

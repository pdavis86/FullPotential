using System;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Events.Args;

namespace FullPotential.Standard.SpecialGear
{
    public class TeleportReloaderEventHandler : IEventHandler
    {
        public Action<IEventHandlerArgs> BeforeEvent => HandleShotFired;

        public Action<IEventHandlerArgs> AfterEvent => null;

        private void HandleShotFired(IEventHandlerArgs eventArgs)
        {
            var shotFiredEventArgs = (ShotFiredEventArgs) eventArgs;

            var fighter = shotFiredEventArgs.Fighter;
            var handStatus = fighter.GetHandStatus(shotFiredEventArgs.IsLeftHand);

            var ammoTypeId = handStatus.EquippedWeapon.GetAmmoTypeId();
            var ammoMax = handStatus.EquippedWeapon.GetAmmoMax();
            var ammoNeeded = ammoMax - handStatus.EquippedWeapon.Ammo;
            
            //todo: zzz v0.5 take money for each item
            fighter.Inventory.TakeItemStack(ammoTypeId, ammoNeeded);

            handStatus.EquippedWeapon.Ammo = ammoMax;
        }
    }
}

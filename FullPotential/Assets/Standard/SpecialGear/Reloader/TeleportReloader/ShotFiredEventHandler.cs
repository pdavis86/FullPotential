using System;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Standard.SpecialGear.Reloader.TeleportReloader
{
    public class ShotFiredEventHandler : IEventHandler
    {
        public Action<IEventHandlerArgs> BeforeHandler => HandleShotFired;

        public Action<IEventHandlerArgs> AfterHandler => null;

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

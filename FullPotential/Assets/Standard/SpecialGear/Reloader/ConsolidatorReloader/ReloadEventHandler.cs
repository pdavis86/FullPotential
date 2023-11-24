using System;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Standard.SpecialGear.Reloader.ConsolidatorReloader
{
    public class ReloadEventHandler : IEventHandler
    {
        public Action<IEventHandlerArgs> BeforeHandler => HandleReloadBefore;

        public Action<IEventHandlerArgs> AfterHandler => null;

        private void HandleReloadBefore(IEventHandlerArgs eventArgs)
        {
            eventArgs.IsDefaultHandlerCancelled = true;

            var reloadEventArgs = (ReloadEventArgs)eventArgs;

            reloadEventArgs.GetNewAmmoCount = () =>
            {
                var fighter = reloadEventArgs.Fighter;
                var handStatus = fighter.GetHandStatus(reloadEventArgs.IsLeftHand);

                var ammoTypeId = handStatus.EquippedWeapon.GetAmmoTypeId();
                var ammoNeeded = handStatus.EquippedWeapon.GetAmmoMax() - handStatus.EquippedWeapon.Ammo;
                return handStatus.EquippedWeapon.Ammo + fighter.Inventory.TakeItemStack(ammoTypeId, ammoNeeded)?.Count ?? 0;
            };

            reloadEventArgs.Fighter.Reload(reloadEventArgs);
        }
    }
}

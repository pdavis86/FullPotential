using System;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Events.Args;

namespace FullPotential.Standard.SpecialGear
{
    public class ConsolidatorReloaderEventHandler : IEventHandler
    {
        public Action<IEventHandlerArgs> BeforeEvent => HandleReloadStartBefore;

        public Action<IEventHandlerArgs> AfterEvent => null;

        private void HandleReloadStartBefore(IEventHandlerArgs eventArgs)
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

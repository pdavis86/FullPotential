using System;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Gameplay.Events.Args;

namespace FullPotential.Standard.EventHandlers
{
    public class ReloaderEventHandler : IEventHandler
    {
        public Action<IEventHandlerArgs> BeforeEvent => HandleReloadStartBefore;

        public Action<IEventHandlerArgs> AfterEvent => null;

        private void HandleReloadStartBefore(IEventHandlerArgs eventArgs)
        {
            //todo: only fire if ConsolidatorReloader is equipped

            eventArgs.IsDefaultHandlerCancelled = true;

            var reloadEventArgs = (ReloadEventArgs)eventArgs;

            var fighter = reloadEventArgs.Fighter;
            var handStatus = fighter.GetHandStatus(reloadEventArgs.IsLeftHand);

            var ammoTypeId = handStatus.EquippedWeapon.GetAmmoTypeId();
            var ammoNeeded = handStatus.EquippedWeapon.GetAmmoMax() - reloadEventArgs.CurrentAmmoCount;
            reloadEventArgs.NewAmmoCount = reloadEventArgs.CurrentAmmoCount + fighter.Inventory.TakeItemStack(ammoTypeId, ammoNeeded)?.Count ?? 0;

            reloadEventArgs.Fighter.Reload(reloadEventArgs);
        }
    }
}

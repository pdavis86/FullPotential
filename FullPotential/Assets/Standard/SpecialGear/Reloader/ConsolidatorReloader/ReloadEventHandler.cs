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
            var reloadEventArgs = (ReloadEventArgs)eventArgs;

            var reloader = (Api.Items.Types.SpecialGear)reloadEventArgs.Fighter.Inventory.GetItemInSlot(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (!reloadEventArgs.Fighter.ConsumeResource(reloader))
            {
                return;
            }

            eventArgs.IsDefaultHandlerCancelled = true;

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

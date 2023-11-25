using System;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Standard.SpecialGear.Reloader.TeleportReloader
{
    public class ReloadEventHandler : IEventHandler
    {
        public Action<IEventHandlerArgs> BeforeHandler => HandleReloadBefore;

        public Action<IEventHandlerArgs> AfterHandler => null;

        private void HandleReloadBefore(IEventHandlerArgs eventArgs)
        {
            var reloadEventArgs = (ReloadEventArgs)eventArgs;

            var reloader = (Api.Items.Types.SpecialGear)reloadEventArgs.Fighter.Inventory.GetItemInSlot(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (!reloadEventArgs.Fighter.ConsumeResource(reloader, slowDrain: true, isTest: true))
            {
                return;
            }

            eventArgs.IsDefaultHandlerCancelled = true;
        }
    }
}

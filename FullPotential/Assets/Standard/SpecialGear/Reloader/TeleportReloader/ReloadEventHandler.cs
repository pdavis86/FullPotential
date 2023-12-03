using System;
using FullPotential.Api.Gameplay.Combat.EventArgs;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader.TeleportReloader
{
    public class ReloadEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Action<IEventHandlerArgs> BeforeHandler => HandleReloadBefore;

        public Action<IEventHandlerArgs> AfterHandler => null;

        private void HandleReloadBefore(IEventHandlerArgs eventArgs)
        {
            var reloadEventArgs = (ReloadEventArgs)eventArgs;

            var reloader = (Api.Items.Types.SpecialGear)reloadEventArgs.Fighter.Inventory.GetItemInSlot(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryTypeId != TeleportReloader.TypeIdString)
            {
                return;
            }

            if (!reloadEventArgs.Fighter.ConsumeResource(reloader, slowDrain: true, isTest: true))
            {
                return;
            }

            eventArgs.IsDefaultHandlerCancelled = true;
        }
    }
}

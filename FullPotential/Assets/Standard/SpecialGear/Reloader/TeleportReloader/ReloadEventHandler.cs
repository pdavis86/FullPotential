using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader.TeleportReloader
{
    public class ReloadEventHandler : IEventHandler
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Func<IEventHandlerArgs, UniTask> BeforeHandlerAsync => HandleReloadBeforeAsync;

        public Func<IEventHandlerArgs, UniTask> AfterHandlerAsync => null;

        private UniTask HandleReloadBeforeAsync(IEventHandlerArgs eventArgs)
        {
            var reloadEventArgs = (ReloadEventArgs)eventArgs;

            var reloader = reloadEventArgs.Fighter.Inventory.GetItemInSlot<Api.Items.Types.SpecialGear>(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryTypeId != TeleportReloader.TypeIdString)
            {
                return UniTask.CompletedTask;
            }

            if (!reloadEventArgs.Fighter.ConsumeResource(reloader, slowDrain: true, isTest: true))
            {
                return UniTask.CompletedTask;
            }

            eventArgs.IsDefaultHandlerCancelled = true;

            return UniTask.CompletedTask;
        }
    }
}

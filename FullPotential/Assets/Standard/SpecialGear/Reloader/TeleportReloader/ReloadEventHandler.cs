using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader.TeleportReloader
{
    [SubscribeToEvent(FighterBase.ReloadEventId)]
    public class ReloadEventHandler : IEventHandler<ReloadEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Func<ReloadEventArgs, UniTask> BeforeHandlerAsync => HandleReloadBeforeAsync;

        public Func<ReloadEventArgs, UniTask> AfterHandlerAsync => null;

        private UniTask HandleReloadBeforeAsync(ReloadEventArgs eventArgs)
        {
            var reloader = eventArgs.Fighter.Inventory.GetItemInSlot<Api.Obsolete.Items.Types.SpecialGear>(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryTypeId != TeleportReloader.TypeIdString)
            {
                return UniTask.CompletedTask;
            }

            if (!eventArgs.Fighter.ConsumeResource(reloader, slowDrain: true, isTest: true))
            {
                return UniTask.CompletedTask;
            }

            eventArgs.IsDefaultHandlerCancelled = true;

            return UniTask.CompletedTask;
        }
    }
}

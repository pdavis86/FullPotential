using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Input;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader.TeleportReloader
{
    public class ReloadEventHandler : IEventHandler<ReloadEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.Before;

        public UniTask<HandlerResult> HandleEventAsync(ReloadEventArgs eventArgs)
        {
            var reloader = eventArgs.Fighter.Inventory.GetItemInSlot<Api.Obsolete.Items.Types.SpecialGear>(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryTypeId != TeleportReloader.TypeIdString)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            if (!eventArgs.Fighter.ConsumeResource(reloader, slowDrain: true, isTest: true))
            {
                return UniTask.FromResult(new HandlerResult());
            }

            return UniTask.FromResult(new HandlerResult(NextAction.Cancel));
        }
    }
}

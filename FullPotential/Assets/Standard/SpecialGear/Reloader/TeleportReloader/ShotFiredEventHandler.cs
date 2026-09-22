using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Input;

using Unity.Netcode;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader.TeleportReloader
{
    public class ShotFiredEventHandler : IEventHandler<ShotFiredEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.After;

        public UniTask<HandlerResult> HandleEventAsync(ShotFiredEventArgs eventArgs)
        {
            var reloader = eventArgs.Fighter.Inventory.GetItemInSlot<Api.Obsolete.Items.Types.SpecialGear>(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryTypeId != TeleportReloader.TypeIdString)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            if (!eventArgs.Fighter.ConsumeResource(reloader, true, !NetworkManager.Singleton.IsServer))
            {
                return UniTask.FromResult(new HandlerResult());
            }

            var fighter = eventArgs.Fighter;

            var reloadEventArgs = new ReloadEventArgs(fighter, eventArgs.SlotId);

            ReloadEventArgs.UpdateAmmoCounts(reloadEventArgs);

            return UniTask.FromResult(new HandlerResult());
        }
    }
}

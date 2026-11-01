using Cysharp.Threading.Tasks;

using FullPotential.Api.EventManagement;
using FullPotential.Api.Input.Events;
using FullPotential.Standard.Weapons.Events;

using Unity.Netcode;

// ReSharper disable ClassNeverInstantiated.Global

using FullPotential.Api.CoreTypeIds;

namespace FullPotential.Standard.CustomGear.Reloader.TeleportReloader
{
    public class ShotFiredEventHandler : IEventHandler<ShotFiredAfterEvent>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.Late;

        public UniTask<HandlerResult> HandleEventAsync(ShotFiredAfterEvent eventArgs)
        {
            var reloader = eventArgs.Fighter.Inventory.GetItemInSlot<Items.CustomGear>(SlotId.FromString(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString));

            if (reloader == null || reloader.RegistryType.TypeId.ToString() != TeleportReloader.TypeIdString)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            if (!eventArgs.Fighter.ConsumeValuePool(reloader, true, !NetworkManager.Singleton.IsServer))
            {
                return UniTask.FromResult(new HandlerResult());
            }

            var fighter = eventArgs.Fighter;

            var reloadEventArgs = new ReloadInputEvent(fighter, eventArgs.SlotId);

            ReloadEventDefaultHandler.UpdateAmmoCounts(reloadEventArgs);

            return UniTask.FromResult(new HandlerResult());
        }
    }
}

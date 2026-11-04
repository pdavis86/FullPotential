using Cysharp.Threading.Tasks;

using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Input.Events;
using FullPotential.Standard.Items;

namespace FullPotential.Standard.CustomGear.Reloader.TeleportReloader
{
    // todo: TeleportReloader should trigger on shot fired (I guess allow a normal reload)
    public class ReloadEventHandler : IEventHandler<ReloadInputEvent>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.Early;

        public UniTask<HandlerResult> HandleEventAsync(ReloadInputEvent eventArgs)
        {
            var reloader = eventArgs.Fighter.Inventory.GetItemInSlot<Items.CustomGear>(SlotId.FromString(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString));

            if (reloader == null || reloader.RegistryType.TypeId.ToString() != TeleportReloader.TypeIdString)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            var itemInSlot = eventArgs.Fighter.Inventory.GetItemInSlot(eventArgs.SlotId);

            if (itemInSlot is not Weapon weapon)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            if (!weapon.IsRanged)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            if (!eventArgs.Fighter.ConsumeValuePool(reloader, slowDrain: true, isTest: true))
            {
                return UniTask.FromResult(new HandlerResult());
            }

            return UniTask.FromResult(new HandlerResult(NextAction.Cancel));
        }
    }
}

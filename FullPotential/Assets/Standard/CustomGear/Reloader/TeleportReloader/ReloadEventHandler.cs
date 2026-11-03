using Cysharp.Threading.Tasks;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.EventManagement;
using FullPotential.Api.Input.Events;
using FullPotential.Api.Utilities.Extensions;
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

            var weapon = eventArgs.Fighter.Inventory.GetItemInSlot<Weapon>(eventArgs.SlotId);

            if (weapon.WeaponType.AmmunitionTypeIdString.IsNullOrWhiteSpace())
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

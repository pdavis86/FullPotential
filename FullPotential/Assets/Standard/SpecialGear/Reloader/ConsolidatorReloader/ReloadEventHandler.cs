using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Input;
using FullPotential.Api.Utilities.Extensions;
using FullPotential.Standard.Items;
using FullPotential.Standard.Weapons.Events;

// ReSharper disable ClassNeverInstantiated.Global

namespace FullPotential.Standard.SpecialGear.Reloader.ConsolidatorReloader
{
    public class ReloadEventHandler : IEventHandler<ReloadInputEvent>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.Early;

        public async UniTask<HandlerResult> HandleEventAsync(ReloadInputEvent eventArgs)
        {
            var reloader = eventArgs.Fighter.Inventory.GetItemInSlot<Items.SpecialGear>(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString);

            if (reloader == null || reloader.RegistryType.TypeId.ToString() != ConsolidatorReloader.TypeIdString)
            {
                return new HandlerResult();
            }

            var weapon = eventArgs.Fighter.Inventory.GetItemInSlot<Weapon>(eventArgs.SlotId);

            if (weapon.WeaponType.AmmunitionTypeIdString.IsNullOrWhiteSpace())
            {
                return new HandlerResult(NextAction.Cancel);
            }

            if (weapon.Ammo == weapon.GetAmmoMax())
            {
                return new HandlerResult(NextAction.Cancel);
            }

            var availableAmmo = eventArgs.Fighter.Inventory.GetItemStackTotal(weapon.WeaponType.AmmunitionTypeIdString);
            if (availableAmmo == 0)
            {
                return new HandlerResult(NextAction.Cancel);
            }

            if (!eventArgs.Fighter.ConsumeResource(reloader))
            {
                return new HandlerResult();
            }

            var slotStatus = eventArgs.Fighter.GetSlotStatus(eventArgs.SlotId);

            slotStatus.SetBusyState(true);

            await UniTask.WaitForSeconds(weapon.GetReloadTime());

            ReloadEventDefaultHandler.UpdateAmmoCounts(eventArgs);

            slotStatus.SetBusyState(false);

            return new HandlerResult(NextAction.Cancel);
        }
    }
}

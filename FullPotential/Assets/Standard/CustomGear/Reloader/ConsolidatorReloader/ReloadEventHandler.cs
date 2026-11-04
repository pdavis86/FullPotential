using Cysharp.Threading.Tasks;

using FullPotential.Api.EventManagement;
using FullPotential.Api.Gameplay.Inventory;
using FullPotential.Api.Input.Events;
using FullPotential.Standard.Items;
using FullPotential.Standard.Weapons.Events;

namespace FullPotential.Standard.CustomGear.Reloader.ConsolidatorReloader
{
    public class ReloadEventHandler : IEventHandler<ReloadInputEvent>
    {
        public NetworkLocation Location => NetworkLocation.Server;

        public Timing Timing => Timing.Early;

        public async UniTask<HandlerResult> HandleEventAsync(ReloadInputEvent eventArgs)
        {
            var reloader = eventArgs.Fighter.Inventory.GetItemInSlot<Items.CustomGear>(SlotId.FromString(SpecialSlots.RangedWeaponReloaderSlot.TypeIdString));

            if (reloader == null || reloader.RegistryType.TypeId.ToString() != ConsolidatorReloader.TypeIdString)
            {
                return new HandlerResult();
            }

            var itemInSlot = eventArgs.Fighter.Inventory.GetItemInSlot(eventArgs.SlotId);

            if (itemInSlot is not Weapon weapon)
            {
                return new HandlerResult();
            }

            if (!weapon.IsRanged)
            {
                return new HandlerResult(NextAction.Cancel);
            }

            if (weapon.Ammo == weapon.GetAmmoMax())
            {
                return new HandlerResult(NextAction.Cancel);
            }

            var availableAmmo = eventArgs.Fighter.Inventory.GetItemStackTotal(weapon.WeaponType.AmmunitionTypeId?.ToString());
            if (availableAmmo == 0)
            {
                return new HandlerResult(NextAction.Cancel);
            }

            if (!eventArgs.Fighter.ConsumeValuePool(reloader))
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

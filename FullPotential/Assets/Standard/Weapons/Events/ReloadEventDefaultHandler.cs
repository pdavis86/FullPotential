using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Input;
using FullPotential.Standard.Items;

namespace FullPotential.Standard.Weapons.Events
{
    public class ReloadEventDefaultHandler : IEventHandler<ReloadInputEvent>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public async UniTask<HandlerResult> HandleEventAsync(ReloadInputEvent eventArgs)
        {
            var itemInSlot = eventArgs.Fighter.Inventory.GetItemInSlot(eventArgs.SlotId);

            if (itemInSlot is not Weapon weapon)
            {
                return new HandlerResult();
            }

            var ammoInInventory = eventArgs.Fighter.Inventory.GetItemStackTotal(weapon.WeaponType.AmmunitionTypeIdString);

            if (ammoInInventory == 0)
            {
                return new HandlerResult();
            }

            var slotStatus = eventArgs.Fighter.GetSlotStatus(eventArgs.SlotId);
            slotStatus.SetBusyState(true);

            //Lose any remaining ammo
            weapon.UpdateAmmo(0);

            await UniTask.WaitForSeconds(weapon.GetReloadTime());

            UpdateAmmoCounts(eventArgs);

            slotStatus.SetBusyState(false);

            return new HandlerResult();
        }

        public static void UpdateAmmoCounts(ReloadInputEvent eventArgs)
        {
            var fighter = eventArgs.Fighter;

            var equippedWeapon = fighter.Inventory.GetItemInSlot<Weapon>(eventArgs.SlotId);

            var ammoTypeId = equippedWeapon.WeaponType.AmmunitionTypeIdString;
            var ammoNeeded = equippedWeapon.GetAmmoMax() - equippedWeapon.Ammo;

            var countTaken = fighter.Inventory.TakeCountFromItemStacks(ammoTypeId, ammoNeeded);

            equippedWeapon.UpdateAmmo(equippedWeapon.Ammo + countTaken);
        }
    }
}

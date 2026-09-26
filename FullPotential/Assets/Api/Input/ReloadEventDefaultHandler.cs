using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Api.Input
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

            ReloadInputEvent.UpdateAmmoCounts(eventArgs);

            slotStatus.SetBusyState(false);

            return new HandlerResult();
        }
    }
}

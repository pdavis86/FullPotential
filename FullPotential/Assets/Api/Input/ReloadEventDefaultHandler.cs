using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Api.Input
{
    public class ReloadEventDefaultHandler : IEventHandler<ReloadEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public Func<ReloadEventArgs, UniTask<HandlerResult>> HandlerAsync => DefaultHandlerAsync;

        private async UniTask<HandlerResult> DefaultHandlerAsync(ReloadEventArgs eventArgs)
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
            slotStatus.IsBusy = true;

            //Lose any remaining ammo
            weapon.UpdateAmmo(0);

            await UniTask.WaitForSeconds(weapon.GetReloadTime());

            ReloadEventArgs.UpdateAmmoCounts(eventArgs);

            slotStatus.IsBusy = false;

            return new HandlerResult();
        }
    }
}

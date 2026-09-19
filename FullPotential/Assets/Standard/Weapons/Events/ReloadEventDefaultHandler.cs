using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Standard.Weapons.Events
{
    public class ReloadEventDefaultHandler : IEventHandler<ReloadEventArgs>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public Func<ReloadEventArgs, UniTask<HandlerResult>> HandlerAsync => DefaultHandlerAsync;

        private async UniTask<HandlerResult> DefaultHandlerAsync(ReloadEventArgs eventArgs)
        {
            var slotStatus = eventArgs.Fighter.GetSlotStatus(eventArgs.SlotId);
            slotStatus.IsBusy = true;

            var weapon = eventArgs.Fighter.Inventory.GetItemInSlot<Weapon>(eventArgs.SlotId);

            //Lose any remaining ammo
            weapon.UpdateAmmo(0);

            await UniTask.WaitForSeconds(weapon.GetReloadTime());

            ReloadEventArgs.UpdateAmmoCounts(eventArgs);

            slotStatus.IsBusy = false;

            return new HandlerResult();
        }
    }
}

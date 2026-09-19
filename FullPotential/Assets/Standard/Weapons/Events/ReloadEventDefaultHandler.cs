using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Standard.Weapons.Events
{
    public class ReloadEventDefaultHandler : IEventHandler<ReloadEvent>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public Func<ReloadEvent, UniTask> HandlerAsync => DefaultHandler;

        private async UniTask DefaultHandler(ReloadEvent eventArgs)
        {
            var slotStatus = eventArgs.Fighter.GetSlotStatus(eventArgs.SlotId);
            slotStatus.IsBusy = true;

            var weapon = eventArgs.Fighter.Inventory.GetItemInSlot<Weapon>(eventArgs.SlotId);

            //Lose any remaining ammo
            weapon.UpdateAmmo(0);

            await UniTask.WaitForSeconds(weapon.GetReloadTime());

            ReloadEvent.UpdateAmmoCounts(eventArgs);

            slotStatus.IsBusy = false;
        }
    }
}

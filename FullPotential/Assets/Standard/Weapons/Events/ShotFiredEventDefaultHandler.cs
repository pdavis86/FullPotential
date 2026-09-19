using System;

using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Combat.Events;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Standard.Weapons.Events
{
    public class ShotFiredEventDefaultHandler : IEventHandler<ShotFiredEvent>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public Func<ShotFiredEvent, UniTask> HandlerAsync => DefaultHandler;

        private UniTask DefaultHandler(ShotFiredEvent eventArgs)
        {
            if (!eventArgs.Fighter.IsServer)
            {
                return UniTask.CompletedTask;
            }

            var fighter = eventArgs.Fighter;

            var equippedWeapon = fighter.Inventory.GetItemInSlot<Weapon>(eventArgs.SlotId);

            equippedWeapon.UpdateAmmo(equippedWeapon.Ammo - eventArgs.AmmoUsed);
            equippedWeapon.IsDirty = true;

            return UniTask.CompletedTask;
        }
    }
}

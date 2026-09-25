using Cysharp.Threading.Tasks;

using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Input;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Standard.Weapons.Events
{
    public class ShotFiredEventDefaultHandler : IEventHandler<ShotFiredAfterEvent>
    {
        public NetworkLocation Location => NetworkLocation.Both;

        public Timing Timing => Timing.Main;

        public UniTask<HandlerResult> HandleEventAsync(ShotFiredAfterEvent eventArgs)
        {
            if (!eventArgs.Fighter.IsServer)
            {
                return UniTask.FromResult(new HandlerResult());
            }

            var fighter = eventArgs.Fighter;

            var equippedWeapon = fighter.Inventory.GetItemInSlot<Weapon>(eventArgs.SlotId);

            equippedWeapon.UpdateAmmo(equippedWeapon.Ammo - eventArgs.AmmoUsed.Value);
            equippedWeapon.IsDirty = true;

            return UniTask.FromResult(new HandlerResult());
        }
    }
}

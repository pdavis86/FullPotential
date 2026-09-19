using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Api.Gameplay.Combat.Events
{
    // todo: move to standard
    [RegisterEvent("2337f94e-5a7d-4e02-b1c8-1b5e9934a3ce")]
    public class ReloadEvent : IEvent
    {
        public bool IsCancelled { get; set; }

        public FighterBase Fighter { get; }

        public string SlotId { get; }

        public ReloadEvent(FighterBase fighter, string slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
        }

        public static void UpdateAmmoCounts(ReloadEvent eventArgs)
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

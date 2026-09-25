using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Events;
using FullPotential.Api.Obsolete.Items.Types;

namespace FullPotential.Api.Input
{
    [RegisterEvent]
    public readonly struct ReloadInputEvent : IEvent
    {
        public FighterBase Fighter { get; }

        public string SlotId { get; }

        public ReloadInputEvent(FighterBase fighter, string slotId)
        {
            Fighter = fighter;
            SlotId = slotId;
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

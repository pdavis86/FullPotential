using FullPotential.Api.Gameplay.Behaviours;

namespace FullPotential.Standard.Weapons.Helpers
{
    public interface IAttackHelper
    {
        void AttackWithItemInHand(FighterBase fighter, string slotId, bool isAutoFire = false);
    }
}

using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Behaviours;

namespace FullPotential.Standard.Weapons.Helpers
{
    public interface IAttackHelper : IService
    {
        void AttackWithItemInHand(FighterBase fighter, string slotId, bool isAutoFire = false);
    }
}

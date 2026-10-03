using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Assets.Api.GameManagement;

namespace FullPotential.Standard.Weapons.Helpers
{
    public interface IAttackHelper : IService
    {
        void AttackWithItemInHand(FighterBase fighter, string slotId, bool isAutoFire = false);
    }
}

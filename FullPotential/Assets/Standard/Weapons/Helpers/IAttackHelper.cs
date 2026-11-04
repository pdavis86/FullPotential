using FullPotential.Api.GameManagement;
using FullPotential.Api.Gameplay.Behaviours;
using FullPotential.Api.Gameplay.Inventory;

namespace FullPotential.Standard.Weapons.Helpers
{
    public interface IAttackHelper : IService
    {
        void AttackWithItemInHand(FighterBase fighter, SlotId slotId, bool isAutoFire = false);
    }
}

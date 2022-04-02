using FullPotential.Api.Gameplay.Combat;

namespace FullPotential.Api.Gameplay
{
    public interface IAttackHelper
    {
        void CheckIfOffTheMap(IDamageable damageable, float yValue);

        string GetDeathMessage(bool isOwner, string victimName, string killerName, string itemName);
    }
}

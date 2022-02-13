using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry.Base;
using UnityEngine;

namespace FullPotential.Api.Gameplay
{
    public interface IAttackHelper
    {
        void DealDamage(
            GameObject source,
            ItemBase itemUsed,
            GameObject target,
            Vector3? position
        );

        void CheckIfOffTheMap(IDamageable damageable, float yValue);

        string GetDeathMessage(bool isOwner, string victimName, string killerName, string itemName);
    }
}

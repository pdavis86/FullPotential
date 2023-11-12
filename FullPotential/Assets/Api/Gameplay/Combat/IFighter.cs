using FullPotential.Api.Gameplay.Events.Args;
using FullPotential.Api.Items.Base;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;
using UnityEngine;

// ReSharper disable UnusedParameter.Global

namespace FullPotential.Api.Gameplay.Combat
{
    public interface IFighter : IDefensible, IDamageable
    {
        Transform Transform { get; }

        GameObject GameObject { get; }

        Rigidbody RigidBody { get; }

        Transform LookTransform { get; }

        string FighterName { get; }

        ulong OwnerClientId { get; }

        void AddAttributeModifier(IAttributeEffect attributeEffect, ItemForCombatBase itemUsed, float effectPercentage);

        void ApplyPeriodicActionToStat(IStatEffect statEffect, ItemForCombatBase itemUsed, IFighter sourceFighter, float effectPercentage);

        void ApplyStatValueChange(IStatEffect statEffect, ItemForCombatBase itemUsed, IFighter sourceFighter, Vector3? position, float effectPercentage);

        void ApplyTemporaryMaxActionToStat(IStatEffect statEffect, ItemForCombatBase itemUsed, IFighter sourceFighter, Vector3? position, float effectPercentage);

        void ApplyElementalEffect(IEffect elementalEffect, ItemForCombatBase itemUsed, IFighter sourceFighter, Vector3? position, float effectPercentage);

        int GetAttributeValue(AttributeAffected attributeAffected);

        float GetCriticalHitChance();

        void Reload(ReloadEventArgs reloadArgs);
    }
}

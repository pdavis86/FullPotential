﻿using FullPotential.Api.Items.Base;
using FullPotential.Api.Registry.Effects;
using UnityEngine;

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

        void AddAttributeModifier(IAttributeEffect attributeEffect, ItemBase itemUsed);

        void ApplyPeriodicActionToStat(IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter);

        void ApplyStatValueChange(IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position);

        void ApplyTemporaryMaxActionToStat(IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position);

        void ApplyElementalEffect(IEffect elementalEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position);
    }
}

﻿using System.Collections.Generic;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Base;
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

        Dictionary<IEffect, float> GetActiveEffects();

        void AddAttributeModifier(IAttributeEffect attributeEffect, Attributes attributes);

        void ApplyPeriodicActionToStat(IStatEffect statEffect, Attributes attributes);

        void ApplyStatValueChange(IStatEffect statEffect, ItemBase itemUsed, IFighter sourceFighter, Vector3? position);

        void ApplyTemporaryMaxActionToStat(IStatEffect statEffect, Attributes attributes);

        void ApplyElementalEffect(IEffect elementalEffect, Attributes attributes);

        void BeginMaintainDistanceOn(GameObject targetGameObject);
    }
}

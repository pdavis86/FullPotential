using System.Collections.Generic;
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

        //void AddAttributeModifier(IAttributeEffect attributeEffect, Attributes attributes);

        //void ApplyPeriodicActionToStat(IStatEffect statEffect, Attributes attributes);

        //void AlterValue(IStatEffect statEffect, Attributes attributes);

        //void ApplyTemporaryMaxActionToStat(IStatEffect statEffect, Attributes attributes);
    }
}

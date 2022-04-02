using FullPotential.Api.Gameplay.Combat;
using FullPotential.Api.Registry;
using FullPotential.Api.Registry.Effects;
using UnityEngine;

namespace FullPotential.Api.Gameplay
{
    public interface IFighter : IDamageable, IDefensible
    {
        void AddAttributeModifier(IAttributeEffect attributeEffect, Attributes attributes);

        void ApplyPeriodicActionToStat(IStatEffect statEffect, Attributes attributes);

        void AlterValue(IStatEffect statEffect, Attributes attributes);

        void ApplyTemporaryMaxActionToStat(IStatEffect statEffect, Attributes attributes);

        Rigidbody GetRigidBody();
    }
}

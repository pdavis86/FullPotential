using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Focus : IStatEffect
    {
        public Guid TypeId => new Guid("d19cde18-e5dd-4fc2-b14f-da7daa5014d4");

        public string TypeName => nameof(Focus);

        public AffectType AffectType => AffectType.TemporaryMaxIncrease;

        public ResourceType StatToAffect => ResourceType.Mana;
    }
}

using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Courage : IStatEffect
    {
        public Guid TypeId => new Guid("ee5271a8-ef14-4f2a-b34b-5ae5a091520f");

        public string TypeName => nameof(Courage);

        public AffectType AffectType => AffectType.TemporaryMaxIncrease;

        public ResourceType StatToAffect => ResourceType.Health;
    }
}

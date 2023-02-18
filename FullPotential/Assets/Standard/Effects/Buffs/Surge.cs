using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Surge : IStatEffect
    {
        public Guid TypeId => new Guid("b1edb96d-da2b-49b3-bac3-80c9b1dfe9d6");

        public string TypeName => nameof(Surge);

        public AffectType AffectType => AffectType.TemporaryMaxIncrease;

        public AffectableStat StatToAffect => AffectableStat.Energy;
    }
}

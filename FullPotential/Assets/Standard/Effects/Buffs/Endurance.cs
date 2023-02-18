using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Endurance : IStatEffect
    {
        public Guid TypeId => new Guid("29fa3077-627c-4cc2-9efc-8293ad7ce52d");

        public string TypeName => nameof(Endurance);

        public AffectType AffectType => AffectType.TemporaryMaxIncrease;

        public AffectableStat StatToAffect => AffectableStat.Stamina;
    }
}

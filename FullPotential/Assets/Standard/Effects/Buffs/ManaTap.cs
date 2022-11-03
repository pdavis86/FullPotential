using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class ManaTap : IStatEffect, IIsSideEffect
    {
        public Guid TypeId => new Guid("06629efd-6a9b-4627-8b02-37e8dab135a1");

        public string TypeName => nameof(ManaTap);

        public Affect Affect => Affect.PeriodicIncrease;

        public AffectableStat StatToAffect => AffectableStat.Mana;
    }
}

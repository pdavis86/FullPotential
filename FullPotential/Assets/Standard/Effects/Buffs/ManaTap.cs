using System;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Effects.Debuffs;

namespace FullPotential.Standard.Effects.Buffs
{
    public class ManaTap : IStatEffect, ISideEffect
    {
        public Guid TypeId => new Guid("06629efd-6a9b-4627-8b02-37e8dab135a1");

        public string TypeName => nameof(ManaTap);

        public Affect Affect => Affect.PeriodicIncrease;

        public Type SideEffectOf => typeof(ManaDrain);

        public AffectableStats? StatToAffect => AffectableStats.Mana;
    }
}

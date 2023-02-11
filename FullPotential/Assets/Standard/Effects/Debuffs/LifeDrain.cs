using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Effects.Buffs;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class LifeDrain : IStatEffect, IHasSideEffect
    {
        public Guid TypeId => new Guid("fe828022-b20d-4204-a6ff-8fada0e75bb5");

        public string TypeName => nameof(LifeDrain);

        public Affect Affect => Affect.PeriodicDecrease;

        public AffectableStat StatToAffect => AffectableStat.Health;

        public Type SideEffectType => typeof(LifeTap);
    }
}

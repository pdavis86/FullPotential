using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Effects.Buffs;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class ManaDrain : IStatEffect, IHasSideEffect
    {
        public Guid TypeId => new Guid("e1ab10b2-fcae-4f25-a11f-ac5aeeadbdce");

        public string TypeName => nameof(ManaDrain);

        public Affect Affect => Affect.PeriodicDecrease;

        public AffectableStat StatToAffect => AffectableStat.Mana;

        public Type SideEffectType => typeof(ManaTap);
    }
}

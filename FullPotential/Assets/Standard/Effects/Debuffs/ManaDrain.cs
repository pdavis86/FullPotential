using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class ManaDrain : IStatEffect
    {
        public Guid TypeId => new Guid("e1ab10b2-fcae-4f25-a11f-ac5aeeadbdce");

        public string TypeName => nameof(ManaDrain);

        public Affect Affect => Affect.PeriodicDecrease;

        public AffectableStats? StatToAffect => AffectableStats.Mana;
    }
}

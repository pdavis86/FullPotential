using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Distract : IStatEffect
    {
        public Guid TypeId => new Guid("fb2fcd58-8a90-46de-8368-731773230835");

        public string TypeName => nameof(Distract);

        public Affect Affect => Affect.TemporaryMaxDecrease;

        public AffectableStats StatToAffect => AffectableStats.Mana;
    }
}

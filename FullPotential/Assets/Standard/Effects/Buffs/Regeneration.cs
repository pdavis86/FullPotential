using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Regeneration : IStatEffect
    {
        public Guid TypeId => new Guid("158d1f0e-994d-49f7-a649-33ff336b8309");

        public string TypeName => nameof(Regeneration);

        public Affect Affect => Affect.PeriodicIncrease;

        public AffectableStats StatToAffect => AffectableStats.Health;
    }
}

using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Support
{
    public class Hurt : IStatEffect
    {
        public Guid TypeId => new Guid("ba71a9bf-87be-420d-ad8b-3412b62be27c");

        public string TypeName => nameof(Hurt);

        public Affect Affect => Affect.SingleDecrease;

        public AffectableStats StatToAffect => AffectableStats.Health;
    }
}

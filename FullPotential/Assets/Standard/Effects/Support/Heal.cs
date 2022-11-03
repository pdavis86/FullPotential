using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Support
{
    public class Heal : IStatEffect
    {
        public Guid TypeId => new Guid("091e97b6-e3c1-4fa0-961c-cbf831e755b5");

        public string TypeName => nameof(Heal);

        public Affect Affect => Affect.SingleIncrease;

        public AffectableStat StatToAffect => AffectableStat.Health;
    }
}

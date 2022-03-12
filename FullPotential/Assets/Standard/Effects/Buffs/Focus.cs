using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Focus : IStatEffect
    {
        public Guid TypeId => new Guid("d19cde18-e5dd-4fc2-b14f-da7daa5014d4");

        public string TypeName => nameof(Focus);

        public Affect Affect => Affect.TemporaryMaxIncrease;

        public AffectableStats? StatToAffect => AffectableStats.Mana;
    }
}

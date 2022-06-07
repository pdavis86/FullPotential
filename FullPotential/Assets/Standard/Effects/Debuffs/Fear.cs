using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Fear : IStatEffect
    {
        public Guid TypeId => new Guid("cc67431d-09d7-4c62-9fde-2dc5aa596ac7");

        public string TypeName => nameof(Fear);

        public Affect Affect => Affect.TemporaryMaxDecrease;

        public AffectableStat StatToAffect => AffectableStat.Health;
    }
}

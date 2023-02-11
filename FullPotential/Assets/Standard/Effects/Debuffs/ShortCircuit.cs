using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class ShortCircuit : IStatEffect
    {
        public Guid TypeId => new Guid("434fc4a7-dcbc-44b8-86d6-5815a42eea0b");

        public string TypeName => nameof(ShortCircuit);

        public Affect Affect => Affect.TemporaryMaxDecrease;

        public AffectableStat StatToAffect => AffectableStat.Energy;
    }
}

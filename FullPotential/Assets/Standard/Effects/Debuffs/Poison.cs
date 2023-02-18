using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Poison : IStatEffect
    {
        public Guid TypeId => new Guid("756a664d-dcd5-4b01-9e42-bf2f6d2a9f0f");

        public string TypeName => nameof(Poison);

        public AffectType AffectType => AffectType.PeriodicDecrease;

        public AffectableStat StatToAffect => AffectableStat.Health;
    }
}

using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Regeneration : IResourceEffect
    {
        public Guid TypeId => new Guid("158d1f0e-994d-49f7-a649-33ff336b8309");

        public AffectType AffectType => AffectType.PeriodicIncrease;

        public Guid ResourceTypeId => ResourceTypeIds.Health;
    }
}

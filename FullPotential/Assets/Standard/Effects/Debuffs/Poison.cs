using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Poison : IResourceEffect
    {
        public Guid TypeId => new Guid("756a664d-dcd5-4b01-9e42-bf2f6d2a9f0f");

        public AffectType AffectType => AffectType.PeriodicDecrease;

        public Guid ResourceTypeId => ResourceTypeIds.Health;
    }
}

using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Standard.Effects.Support
{
    public class Heal : IResourceEffect
    {
        public Guid TypeId => new Guid("091e97b6-e3c1-4fa0-961c-cbf831e755b5");

        public AffectType AffectType => AffectType.SingleIncrease;

        public Guid ResourceTypeId => ResourceTypeIds.Health;
    }
}

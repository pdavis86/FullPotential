using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Effects
{
    public class Hurt : IResourceEffect
    {
        public Guid TypeId => new Guid("ba71a9bf-87be-420d-ad8b-3412b62be27c");

        public AffectType AffectType => AffectType.SingleDecrease;

        public Guid ResourceTypeId => ResourceTypeIds.Health;
    }
}

using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Courage : IResourceEffect
    {
        public Guid TypeId => new Guid("ee5271a8-ef14-4f2a-b34b-5ae5a091520f");

        public AffectType AffectType => AffectType.TemporaryMaxIncrease;

        public Guid ResourceTypeId => ResourceTypeIds.Health;
    }
}

using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Resources;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Surge : IResourceEffect
    {
        public Guid TypeId => new Guid("b1edb96d-da2b-49b3-bac3-80c9b1dfe9d6");

        public AffectType AffectType => AffectType.TemporaryMaxIncrease;

        public Guid ResourceTypeId => Energy.Id;
    }
}

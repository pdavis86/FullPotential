using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Endurance : IResourceEffect
    {
        public Guid TypeId => new Guid("29fa3077-627c-4cc2-9efc-8293ad7ce52d");

        public AffectType AffectType => AffectType.TemporaryMaxIncrease;

        public Guid ResourceTypeId => ResourceTypeIds.Stamina;
    }
}

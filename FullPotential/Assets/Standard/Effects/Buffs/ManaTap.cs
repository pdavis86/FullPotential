using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class ManaTap : IResourceEffect, IIsSideEffect
    {
        public const string TypeIdString = "06629efd-6a9b-4627-8b02-37e8dab135a1";

        public Guid TypeId => new Guid(TypeIdString);

        public AffectType AffectType => AffectType.PeriodicIncrease;

        public Guid ResourceTypeId => ResourceTypeIds.Mana;
    }
}

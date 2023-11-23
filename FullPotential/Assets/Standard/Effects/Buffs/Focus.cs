using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Focus : IResourceEffect
    {
        public Guid TypeId => new Guid("d19cde18-e5dd-4fc2-b14f-da7daa5014d4");

        public AffectType AffectType => AffectType.TemporaryMaxIncrease;

        public Guid ResourceTypeId => ResourceTypeIds.Mana;
    }
}

using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Distract : IResourceEffect
    {
        public Guid TypeId => new Guid("fb2fcd58-8a90-46de-8368-731773230835");

        public AffectType AffectType => AffectType.TemporaryMaxDecrease;

        public Guid ResourceTypeId => ResourceTypeIds.Mana;
    }
}

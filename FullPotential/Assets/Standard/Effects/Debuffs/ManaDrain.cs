using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Standard.Effects.Buffs;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class ManaDrain : IResourceEffect, IHasSideEffect
    {
        public Guid TypeId => new Guid("e1ab10b2-fcae-4f25-a11f-ac5aeeadbdce");

        public AffectType AffectType => AffectType.PeriodicDecrease;

        public Guid ResourceTypeId => ResourceTypeIds.Mana;

        public Guid SideEffectTypeId => new Guid(ManaTap.TypeIdString);
    }
}

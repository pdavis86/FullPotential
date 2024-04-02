using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Effects.Buffs;
using FullPotential.Standard.Resources;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class ManaDrain : IResourceEffect, IHasSideEffect
    {
        public Guid TypeId => new Guid("e1ab10b2-fcae-4f25-a11f-ac5aeeadbdce");

        public AffectType AffectType => AffectType.PeriodicDecrease;

        public Guid ResourceTypeId => Mana.Id;

        public Guid SideEffectTypeId => new Guid(ManaTap.TypeIdString);
    }
}

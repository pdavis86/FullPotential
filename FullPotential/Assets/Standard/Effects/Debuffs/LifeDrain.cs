using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Effects.Buffs;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class LifeDrain : IResourceEffect, IHasSideEffect
    {
        public Guid TypeId => new Guid("fe828022-b20d-4204-a6ff-8fada0e75bb5");

        public AffectType AffectType => AffectType.PeriodicDecrease;

        public Guid ResourceTypeId => ResourceTypeIds.Health;

        public Guid SideEffectTypeId => new Guid(LifeTap.TypeIdString);
    }
}

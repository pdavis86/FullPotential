using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Effects.Buffs;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class LifeDrain : IResourceEffect, IHasSideEffect
    {
        private static readonly Guid Id = new Guid("fe828022-b20d-4204-a6ff-8fada0e75bb5");

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.PeriodicDecrease;

        public string ResourceTypeIdString => ResourceTypeIds.HealthId;

        public string SideEffectTypeIdString => LifeTap.TypeIdString;
    }
}

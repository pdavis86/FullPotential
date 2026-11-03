using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Effects.Buffs;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class LifeDrain : IValuePoolEffectType, IHasSideEffect
    {
        public Guid TypeId { get; } = new Guid("fe828022-b20d-4204-a6ff-8fada0e75bb5");

        public EffectActionType EffectActionType => EffectActionType.PeriodicDecrease;

        public ValuePoolTypeId ValuePoolTypeId => ValuePoolTypeIds.HealthId;

        public EffectTypeId SideEffectTypeId => LifeTap.Id;
    }
}

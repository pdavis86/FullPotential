using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Regeneration : IValuePoolEffectType
    {
        public Guid TypeId { get; } = new Guid("158d1f0e-994d-49f7-a649-33ff336b8309");

        public EffectActionType EffectActionType => EffectActionType.PeriodicIncrease;

        public string ValuePoolTypeIdString => ValuePoolTypeIds.HealthId;
    }
}

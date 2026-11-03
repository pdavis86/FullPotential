using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Poison : IValuePoolEffectType
    {
        public Guid TypeId { get; } = new Guid("756a664d-dcd5-4b01-9e42-bf2f6d2a9f0f");

        public EffectActionType EffectActionType => EffectActionType.PeriodicDecrease;

        public string ValuePoolTypeIdString => ValuePoolTypeIds.HealthId;
    }
}

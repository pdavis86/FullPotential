using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Resilience : IValuePoolEffectType
    {
        public Guid TypeId { get; } = new Guid("ee5271a8-ef14-4f2a-b34b-5ae5a091520f");

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxIncrease;

        public string ValuePoolTypeIdString => ValuePoolTypeIds.HealthId;
    }
}

using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Endurance : IValuePoolEffectType
    {
        public Guid TypeId { get; } = new Guid("29fa3077-627c-4cc2-9efc-8293ad7ce52d");

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxIncrease;

        public string ValuePoolTypeIdString => ValuePoolTypeIds.StaminaId;
    }
}

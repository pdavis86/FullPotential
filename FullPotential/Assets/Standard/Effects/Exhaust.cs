using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Standard.Effects
{
    public class Exhaust : IValuePoolEffectType
    {
        public const string TypeIdString = "5b6ccdf4-261f-440d-9033-35a63ca6fc52";

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public EffectActionType EffectActionType => EffectActionType.SingleDecrease;

        public ValuePoolTypeId ValuePoolTypeId => ValuePoolTypeIds.StaminaId;
    }
}

using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Core.Registry.Effects
{
    public class Hurt : IValuePoolEffectType
    {
        public Guid TypeId { get; } = new Guid(EffectTypeIds.HurtId);

        public EffectActionType EffectActionType => EffectActionType.SingleDecrease;

        public ValuePoolTypeId ValuePoolTypeId => ValuePoolTypeIds.HealthId;
    }
}

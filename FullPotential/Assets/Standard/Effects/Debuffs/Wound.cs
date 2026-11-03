using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Wound : IValuePoolEffectType
    {
        public Guid TypeId { get; } = new Guid("cc67431d-09d7-4c62-9fde-2dc5aa596ac7");

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxDecrease;

        public ValuePoolTypeId ValuePoolTypeId => ValuePoolTypeIds.HealthId;
    }
}

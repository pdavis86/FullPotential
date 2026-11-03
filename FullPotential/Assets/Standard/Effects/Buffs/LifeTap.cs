using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class LifeTap : IValuePoolEffectType, IIsSideEffect
    {
        public const string TypeIdString = "eabd80bd-e4aa-4d58-be24-9ec8106b2c9c";

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public EffectActionType EffectActionType => EffectActionType.PeriodicIncrease;

        public ValuePoolTypeId ValuePoolTypeId => ValuePoolTypeIds.HealthId;
    }
}

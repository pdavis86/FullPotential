using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class LifeTap : IValuePoolEffectType, IIsSideEffect
    {
        private const string TypeIdString = "eabd80bd-e4aa-4d58-be24-9ec8106b2c9c";

        public static readonly EffectTypeId Id = EffectTypeId.FromString(TypeIdString);

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public EffectActionType EffectActionType => EffectActionType.PeriodicIncrease;

        public ValuePoolTypeId ValuePoolTypeId => ValuePoolTypeIds.HealthId;
    }
}

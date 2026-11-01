using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class LifeTap : IValuePoolEffectType, IIsSideEffect
    {
        public const string TypeIdString = "eabd80bd-e4aa-4d58-be24-9ec8106b2c9c";

        public Guid TypeId => Id;

        private static readonly Guid Id = new Guid(TypeIdString);

        public EffectActionType EffectActionType => EffectActionType.PeriodicIncrease;

        public string ValuePoolTypeIdString => ValuePoolTypeIds.HealthId;
    }
}

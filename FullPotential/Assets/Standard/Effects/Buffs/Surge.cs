using System;

using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Standard.ValuePools;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Surge : IValuePoolEffectType
    {
        public Guid TypeId { get; } = new Guid("b1edb96d-da2b-49b3-bac3-80c9b1dfe9d6");

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxIncrease;

        public ValuePoolTypeId ValuePoolTypeId => Energy.Id;
    }
}

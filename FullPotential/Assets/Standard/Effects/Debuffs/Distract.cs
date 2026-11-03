using System;

using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Standard.ValuePools;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Distract : IValuePoolEffectType
    {
        public Guid TypeId { get; } = new Guid("fb2fcd58-8a90-46de-8368-731773230835");

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxDecrease;

        public ValuePoolTypeId ValuePoolTypeId => Mana.Id;
    }
}

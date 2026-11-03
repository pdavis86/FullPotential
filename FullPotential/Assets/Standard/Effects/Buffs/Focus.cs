using System;

using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;
using FullPotential.Standard.ValuePools;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Focus : IValuePoolEffectType
    {
        public Guid TypeId { get; } = new Guid("d19cde18-e5dd-4fc2-b14f-da7daa5014d4");

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxIncrease;

        public ValuePoolTypeId ValuePoolTypeId => Mana.Id;
    }
}

using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.ValuePools;

namespace FullPotential.Standard.Effects.Buffs
{
    public class ManaTap : IValuePoolEffectType, IIsSideEffect
    {
        public const string TypeIdString = "06629efd-6a9b-4627-8b02-37e8dab135a1";

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public EffectActionType EffectActionType => EffectActionType.PeriodicIncrease;

        public ValuePoolTypeId ValuePoolTypeId => Mana.Id;
    }
}

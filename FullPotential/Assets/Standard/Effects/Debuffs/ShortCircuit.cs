using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.ValuePools;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class ShortCircuit : IValuePoolEffectType
    {
        public Guid TypeId { get; } = new Guid("434fc4a7-dcbc-44b8-86d6-5815a42eea0b");

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxDecrease;

        public ValuePoolTypeId ValuePoolTypeId => Energy.Id;
    }
}

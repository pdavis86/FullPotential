using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Lethargy : IValuePoolEffectType
    {
        public Guid TypeId { get; } = new Guid("260ff724-0708-42fa-81ab-45af911e6daf");

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxDecrease;

        public ValuePoolTypeId ValuePoolTypeId => ValuePoolTypeIds.StaminaId;
    }
}

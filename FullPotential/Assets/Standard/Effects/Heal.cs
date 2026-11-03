using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects
{
    public class Heal : IValuePoolEffectType
    {
        public const string TypeIdString = "091e97b6-e3c1-4fa0-961c-cbf831e755b5";

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public EffectActionType EffectActionType => EffectActionType.SingleIncrease;

        public ValuePoolTypeId ValuePoolTypeId => ValuePoolTypeIds.HealthId;
    }
}

using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Core.Registry.Effects
{
    public class Heal : IResourceEffectType
    {
        private static readonly Guid Id = new Guid(EffectTypeIds.HealId);

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.SingleIncrease;

        public string ResourceTypeIdString => ResourceTypeIds.HealthId;
    }
}

using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Core.Registry.Effects
{
    public class Hurt : IResourceEffectType
    {
        private static readonly Guid Id = new Guid(EffectTypeIds.HurtId);

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.SingleDecrease;

        public string ResourceTypeIdString => ResourceTypeIds.HealthId;
    }
}

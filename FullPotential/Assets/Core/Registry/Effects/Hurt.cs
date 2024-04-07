using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Effects
{
    public class Hurt : IResourceEffect
    {
        private static readonly Guid Id = new Guid(EffectTypeIds.HurtId);

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.SingleDecrease;

        public string ResourceTypeIdString => ResourceTypeIds.HealthId;
    }
}

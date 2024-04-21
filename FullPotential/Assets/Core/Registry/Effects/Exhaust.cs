using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Core.Registry.Effects
{
    public class Exhaust : IResourceEffect
    {
        private static readonly Guid Id = new Guid(EffectTypeIds.ExhaustId);

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.SingleDecrease;

        public string ResourceTypeIdString => ResourceTypeIds.StaminaId;
    }
}

using System;
using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Core.Registry.Effects
{
    public class Invigorate : IResourceEffectType
    {
        private static readonly Guid Id = new Guid(EffectTypeIds.InvigorateId);

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.SingleIncrease;

        public string ResourceTypeIdString => ResourceTypeIds.StaminaId;
    }
}

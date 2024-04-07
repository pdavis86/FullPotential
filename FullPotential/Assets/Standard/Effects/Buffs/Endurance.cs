using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Endurance : IResourceEffect
    {
        private static readonly Guid Id = new Guid("29fa3077-627c-4cc2-9efc-8293ad7ce52d");

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxIncrease;

        public string ResourceTypeIdString => ResourceTypeIds.StaminaId;
    }
}

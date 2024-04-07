using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Courage : IResourceEffect
    {
        private static readonly Guid Id = new Guid("ee5271a8-ef14-4f2a-b34b-5ae5a091520f");

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxIncrease;

        public string ResourceTypeIdString => ResourceTypeIds.HealthId;
    }
}

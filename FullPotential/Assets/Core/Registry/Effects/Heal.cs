using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Resources;

namespace FullPotential.Core.Registry.Effects
{
    public class Heal : IResourceEffect
    {
        private static readonly Guid Id = new Guid("091e97b6-e3c1-4fa0-961c-cbf831e755b5");

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.SingleIncrease;

        public string ResourceTypeIdString => ResourceTypeIds.HealthId;
    }
}

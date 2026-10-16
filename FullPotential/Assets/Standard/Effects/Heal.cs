using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects
{
    public class Heal : IResourceEffectType
    {
        public const string TypeIdString = "091e97b6-e3c1-4fa0-961c-cbf831e755b5";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.SingleIncrease;

        public string ResourceTypeIdString => ResourceTypeIds.HealthId;
    }
}

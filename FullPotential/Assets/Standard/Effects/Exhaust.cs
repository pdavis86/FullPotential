using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects
{
    public class Exhaust : IResourceEffectType
    {
        public const string TypeIdString = "5b6ccdf4-261f-440d-9033-35a63ca6fc52";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.SingleDecrease;

        public string ResourceTypeIdString => ResourceTypeIds.StaminaId;
    }
}

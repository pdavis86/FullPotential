using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects
{
    public class Invigorate : IResourceEffectType
    {
        public const string TypeIdString = "f7843e9c-4914-4e80-a86f-aaf202262b63";

        private static readonly Guid Id = new Guid(TypeIdString);

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.SingleIncrease;

        public string ResourceTypeIdString => ResourceTypeIds.StaminaId;
    }
}

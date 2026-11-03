using System;

using FullPotential.Api.CoreTypeIds;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Api.Registry.Gameplay;

namespace FullPotential.Standard.Effects
{
    public class Invigorate : IValuePoolEffectType
    {
        public const string TypeIdString = "f7843e9c-4914-4e80-a86f-aaf202262b63";

        public Guid TypeId { get; } = new Guid(TypeIdString);

        public EffectActionType EffectActionType => EffectActionType.SingleIncrease;

        public ValuePoolTypeId ValuePoolTypeId => ValuePoolTypeIds.StaminaId;
    }
}

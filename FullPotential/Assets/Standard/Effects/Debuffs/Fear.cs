using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Resources;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Fear : IResourceEffect
    {
        private static readonly Guid Id = new Guid("cc67431d-09d7-4c62-9fde-2dc5aa596ac7");

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxDecrease;

        public string ResourceTypeIdString => ResourceTypeIds.HealthId;
    }
}

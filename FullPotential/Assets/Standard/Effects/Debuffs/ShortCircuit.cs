using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Resources;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class ShortCircuit : IResourceEffectType
    {
        private static readonly Guid Id = new Guid("434fc4a7-dcbc-44b8-86d6-5815a42eea0b");

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxDecrease;

        public string ResourceTypeIdString => Energy.TypeIdString;
    }
}

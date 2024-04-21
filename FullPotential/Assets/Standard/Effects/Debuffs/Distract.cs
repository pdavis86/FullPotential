using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Resources;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class Distract : IResourceEffectType
    {
        private static readonly Guid Id = new Guid("fb2fcd58-8a90-46de-8368-731773230835");

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxDecrease;

        public string ResourceTypeIdString => Mana.TypeIdString;
    }
}

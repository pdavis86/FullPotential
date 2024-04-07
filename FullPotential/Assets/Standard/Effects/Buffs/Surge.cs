using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Resources;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Surge : IResourceEffect
    {
        private static readonly Guid Id = new Guid("b1edb96d-da2b-49b3-bac3-80c9b1dfe9d6");

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxIncrease;

        public string ResourceTypeIdString => Energy.TypeIdString;
    }
}

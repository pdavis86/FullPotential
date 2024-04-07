using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Resources;

namespace FullPotential.Standard.Effects.Buffs
{
    public class ManaTap : IResourceEffect, IIsSideEffect
    {
        public const string TypeIdString = "06629efd-6a9b-4627-8b02-37e8dab135a1";

        public Guid TypeId => Id;

        private static readonly Guid Id = new Guid(TypeIdString);

        public EffectActionType EffectActionType => EffectActionType.PeriodicIncrease;

        public string ResourceTypeIdString => Mana.TypeIdString;
    }
}

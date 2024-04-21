using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Resources;

namespace FullPotential.Standard.Effects.Buffs
{
    public class Focus : IResourceEffectType
    {
        private static readonly Guid Id = new Guid("d19cde18-e5dd-4fc2-b14f-da7daa5014d4");

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.TemporaryMaxIncrease;

        public string ResourceTypeIdString => Mana.TypeIdString;
    }
}

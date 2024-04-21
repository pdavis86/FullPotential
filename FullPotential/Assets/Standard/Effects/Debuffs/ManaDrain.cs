using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Registry.Effects;
using FullPotential.Standard.Effects.Buffs;
using FullPotential.Standard.Resources;

namespace FullPotential.Standard.Effects.Debuffs
{
    public class ManaDrain : IResourceEffectType, IHasSideEffect
    {
        private static readonly Guid Id = new Guid("e1ab10b2-fcae-4f25-a11f-ac5aeeadbdce");

        public Guid TypeId => Id;

        public EffectActionType EffectActionType => EffectActionType.PeriodicDecrease;

        public string ResourceTypeIdString => Mana.TypeIdString;

        public string SideEffectTypeIdString => ManaTap.TypeIdString;
    }
}

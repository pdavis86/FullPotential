using System;
using FullPotential.Api.Gameplay.Effects;
using FullPotential.Api.Obsolete;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class LifeTap : IStatEffect, IIsSideEffect
    {
        public const string TypeIdString = "eabd80bd-e4aa-4d58-be24-9ec8106b2c9c";

        public Guid TypeId => new Guid(TypeIdString);

        public AffectType AffectType => AffectType.PeriodicIncrease;

        public ResourceType StatToAffect => ResourceType.Health;
    }
}

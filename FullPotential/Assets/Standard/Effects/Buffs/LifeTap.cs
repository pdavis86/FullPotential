using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Buffs
{
    public class LifeTap : IEffectBuff
    {
        public Guid TypeId => new Guid("eabd80bd-e4aa-4d58-be24-9ec8106b2c9c");

        public string TypeName => nameof(LifeTap);

        public bool IsSideEffect => true;
    }
}

using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Effects.Buffs
{
    public class LifeTap : IEffectBuff
    {
        public Guid TypeId => new Guid("eabd80bd-e4aa-4d58-be24-9ec8106b2c9c");

        public string TypeName => nameof(LifeTap);

        public bool IsSideEffect => true;
    }
}

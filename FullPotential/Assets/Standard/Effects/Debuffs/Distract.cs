using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Effects.Debuffs
{
    public class Distract : IEffectDebuff
    {
        public Guid TypeId => new Guid("fb2fcd58-8a90-46de-8368-731773230835");

        public string TypeName => nameof(Distract);

        public bool IsSideEffect => false;
    }
}

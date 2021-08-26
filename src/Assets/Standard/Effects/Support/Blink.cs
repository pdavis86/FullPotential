using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Effects.Support
{
    public class Blink : IEffectSupport
    {
        public Guid TypeId => new Guid("f5daeb39-3a24-4920-ae9e-589550bbc3b4");

        public string TypeName => nameof(Blink);

        public bool IsSideEffect => false;
    }
}

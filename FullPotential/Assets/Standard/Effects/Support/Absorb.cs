using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Support
{
    public class Absorb : IEffectSupport
    {
        public Guid TypeId => new Guid("65edbe76-982e-467f-9a4c-c61e7a956a4d");

        public string TypeName => nameof(Absorb);

        public bool IsSideEffect => false;
    }
}

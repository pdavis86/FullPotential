using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Effects.Support
{
    public class Leap : IEffectSupport
    {
        public Guid TypeId => new Guid("a2cb2a03-3684-450d-a1cb-6396dc96ab48");

        public string TypeName => nameof(Leap);

        public bool IsSideEffect => false;
    }
}

using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Effects.Support
{
    public class Reflect : IEffectSupport
    {
        public Guid TypeId => new Guid("7e9f4b8a-2c5f-41cb-b585-e06ab59d2277");

        public string TypeName => nameof(Reflect);

        public bool IsSideEffect => false;
    }
}

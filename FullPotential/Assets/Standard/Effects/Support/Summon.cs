using FullPotential.Assets.Api.Registry;
using System;

namespace FullPotential.Assets.Standard.Effects.Support
{
    public class Summon : IEffectSupport
    {
        public Guid TypeId => new Guid("668f5ab8-0f8d-4f8c-8e88-90eb7367d22a");

        public string TypeName => nameof(Summon);

        public bool IsSideEffect => false;
    }
}

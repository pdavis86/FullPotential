using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Support
{
    public class Soften : IEffectSupport
    {
        public Guid TypeId => new Guid("98593c2f-2008-4895-ab70-da5eaaa31a23");

        public string TypeName => nameof(Soften);

        public bool IsSideEffect => false;
    }
}

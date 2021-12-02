using FullPotential.Api.Registry;
using System;

namespace FullPotential.Standard.Effects.Support
{
    public class Deflect : IEffectSupport
    {
        public Guid TypeId => new Guid("3cf203cb-6d09-4aaf-9960-288d87b0cec5");

        public string TypeName => nameof(Deflect);

        public bool IsSideEffect => false;
    }
}

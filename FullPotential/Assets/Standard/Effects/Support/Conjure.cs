using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Support
{
    public class Conjure : ICustomEffect
    {
        public Guid TypeId => new Guid("0ce5814a-5d53-44fb-995e-b3992480184c");

        public string TypeName => nameof(Conjure);

        public void ApplyEffect()
        {
            throw new NotImplementedException();
        }
    }
}

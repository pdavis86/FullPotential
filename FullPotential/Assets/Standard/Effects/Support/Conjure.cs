using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Support
{
    public class Conjure : ICustomEffectType
    {
        private static readonly Guid Id = new Guid("0ce5814a-5d53-44fb-995e-b3992480184c");

        public Guid TypeId => Id;

        public void ApplyEffect()
        {
            throw new NotImplementedException();
        }
    }
}

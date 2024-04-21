using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Support
{
    public class Float : ICustomEffectType
    {
        private static readonly Guid Id = new Guid("98593c2f-2008-4895-ab70-da5eaaa31a23");

        public Guid TypeId => Id;

        public void ApplyEffect()
        {
            throw new NotImplementedException();
        }
    }
}

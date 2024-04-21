using System;
using FullPotential.Api.Registry.Effects;

namespace FullPotential.Standard.Effects.Support
{
    public class Summon : ICustomEffectType
    {
        private static readonly Guid Id = new Guid("668f5ab8-0f8d-4f8c-8e88-90eb7367d22a");

        public Guid TypeId => Id;

        public void ApplyEffect()
        {
            throw new NotImplementedException();
        }
    }
}
